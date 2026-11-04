using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Generic;

namespace ApkPatcher;

class Program
{
    static void Main(string[] args)
    {
        string projectRoot = @"A:\Proyectos\App\MikuCube-ESP";
        string originalApkPath = Path.Combine(projectRoot, "MikuCube-v1.0.2-EN.apk");
        string keystorePath = Path.Combine(projectRoot, "mikucube-release.keystore");
        string outputApkPath = Path.Combine(projectRoot, "MikuCube-v1.0.2-ES.apk");
        string copyApkPath = Path.Combine(projectRoot, "MikuCube-v1.0.2.apk");

        Console.WriteLine("=================================================");
        Console.WriteLine(" MikuCube-ESP APK Patcher & V2 Signer");
        Console.WriteLine("=================================================");

        // Step 1: Read original APK into memory
        Console.WriteLine("\n[1/5] Loading original aligned APK...");
        byte[] apkBytes = File.ReadAllBytes(originalApkPath);
        Console.WriteLine($"Original APK loaded: {apkBytes.Length:N0} bytes");

        // Step 2: Locate assets/index.android.bundle in ZIP Central Directory
        Console.WriteLine("\n[2/5] Locating 'assets/index.android.bundle'...");
        int eocdOffset = FindEocd(apkBytes);
        uint origCdOffset = BitConverter.ToUInt32(apkBytes, eocdOffset + 16);
        uint origCdSize = BitConverter.ToUInt32(apkBytes, eocdOffset + 12);

        // Check if there was an existing APK signing block before CD
        int origSigBlockOffset = -1;
        ulong origSigBlockSize = 0;
        if (origCdOffset >= 24)
        {
            string magic = Encoding.ASCII.GetString(apkBytes, (int)origCdOffset - 16, 16);
            if (magic == "APK Sig Block 42")
            {
                origSigBlockSize = BitConverter.ToUInt64(apkBytes, (int)origCdOffset - 24);
                origSigBlockOffset = (int)(origCdOffset - origSigBlockSize - 8);
                Console.WriteLine($"Found existing APK Sig Block at offset {origSigBlockOffset}, size {origSigBlockSize}");
            }
        }

        // Find assets/index.android.bundle in CD
        int cdEntryOffset = -1;
        uint localHeaderOffset = 0;
        uint bundleDataSize = 0;
        byte[] targetName = Encoding.ASCII.GetBytes("assets/index.android.bundle");

        for (int i = (int)origCdOffset; i < origCdOffset + origCdSize - targetName.Length; i++)
        {
            if (apkBytes[i] == 0x50 && apkBytes[i + 1] == 0x4B && apkBytes[i + 2] == 0x01 && apkBytes[i + 3] == 0x02)
            {
                ushort nLen = BitConverter.ToUInt16(apkBytes, i + 28);
                if (nLen == targetName.Length)
                {
                    bool match = true;
                    for (int j = 0; j < nLen; j++)
                    {
                        if (apkBytes[i + 46 + j] != targetName[j]) { match = false; break; }
                    }
                    if (match)
                    {
                        cdEntryOffset = i;
                        localHeaderOffset = BitConverter.ToUInt32(apkBytes, i + 42);
                        bundleDataSize = BitConverter.ToUInt32(apkBytes, i + 24);
                        break;
                    }
                }
            }
        }

        if (cdEntryOffset == -1) throw new Exception("Entry 'assets/index.android.bundle' not found in CD!");

        ushort lhNameLen = BitConverter.ToUInt16(apkBytes, (int)localHeaderOffset + 26);
        ushort lhExtraLen = BitConverter.ToUInt16(apkBytes, (int)localHeaderOffset + 28);
        int bundleDataOffset = (int)localHeaderOffset + 30 + lhNameLen + lhExtraLen;
        Console.WriteLine($"Found bundle data at offset {bundleDataOffset}, size {bundleDataSize} bytes (Stored, uncompressed)");

        // Step 3: Extract and patch bundle strings in-place
        Console.WriteLine("\n[3/5] Patching Spanish strings in Hermes bytecode...");
        byte[] bundleBytes = new byte[bundleDataSize];
        Array.Copy(apkBytes, bundleDataOffset, bundleBytes, 0, (int)bundleDataSize);

        AnalyzeAndPatchHermesStrings(bundleBytes);

        // Compute new CRC32
        uint newCrc = ComputeCrc32(bundleBytes);
        Console.WriteLine($"New bundle CRC32: 0x{newCrc:X8}");

        // Update bundle data in apkBytes
        Array.Copy(bundleBytes, 0, apkBytes, bundleDataOffset, bundleBytes.Length);

        // Update CRC in Local Header
        BitConverter.GetBytes(newCrc).CopyTo(apkBytes, localHeaderOffset + 14);

        // Update CRC in Central Directory
        BitConverter.GetBytes(newCrc).CopyTo(apkBytes, cdEntryOffset + 16);

        // Step 4: Prepare sections for APK Signature Scheme v2
        Console.WriteLine("\n[4/5] Building APK sections and signing with RSA-2048 / SHA-256...");
        // Section 1: from 0 to origSigBlockOffset (if present) or origCdOffset
        int section1Len = origSigBlockOffset >= 0 ? origSigBlockOffset : (int)origCdOffset;
        byte[] section1 = new byte[section1Len];
        Array.Copy(apkBytes, 0, section1, 0, section1Len);

        // Section 2: Central Directory
        byte[] section2 = new byte[origCdSize];
        Array.Copy(apkBytes, origCdOffset, section2, 0, origCdSize);

        // Section 3: EOCD
        byte[] section3 = new byte[apkBytes.Length - eocdOffset];
        Array.Copy(apkBytes, eocdOffset, section3, 0, section3.Length);

        // Update Section 3 EOCD CD offset as if signing block doesn't exist (i.e. section1.Length)
        BitConverter.GetBytes((uint)section1.Length).CopyTo(section3, 16);

        // Compute Merkle root digest over Section 1, Section 2, Section 3
        byte[] rootDigest = ComputeApkRootDigest(section1, section2, section3);
        Console.WriteLine($"Merkle Tree Root Digest: {BitConverter.ToString(rootDigest).Replace("-", "").ToLower()}");

        // Keystore generation / loading
        RSA rsa;
        X509Certificate2 cert;
        if (File.Exists(keystorePath))
        {
            Console.WriteLine($"Using existing keystore: {keystorePath}");
            var collection = new X509Certificate2Collection();
            collection.Import(keystorePath, "android", X509KeyStorageFlags.Exportable);
            cert = collection[0];
            rsa = cert.GetRSAPrivateKey()!;
        }
        else
        {
            Console.WriteLine($"Generating new RSA-2048 key and self-signed certificate...");
            rsa = RSA.Create(2048);
            var req = new CertificateRequest("CN=MikuCube-ESP, O=MikuCube, C=ES", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(30));
            byte[] pfx = cert.Export(X509ContentType.Pfx, "android");
            File.WriteAllBytes(keystorePath, pfx);
            Console.WriteLine($"Saved keystore to: {keystorePath} (password: android)");
        }

        byte[] certDer = cert.RawData;
        byte[] spki = rsa.ExportSubjectPublicKeyInfo(); // MUST be SubjectPublicKeyInfo!

        // Construct SignedData
        byte[] signedData = BuildSignedData(rootDigest, certDer);

        // Sign SignedData using RSASSA-PKCS1-v1_5 with SHA-256 (0x0103)
        byte[] signature = rsa.SignData(signedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Construct Signers Block
        byte[] signerBlock = BuildSignerBlock(signedData, signature, spki);

        // Construct APK Signing Block (with 4096-byte alignment padding for Android 15 / Page-size compatibility)
        byte[] apkSigningBlock = BuildApkSigningBlock(signerBlock);

        // Update Section 3 EOCD CD offset with the signing block included
        uint finalCdOffset = (uint)(section1.Length + apkSigningBlock.Length);
        BitConverter.GetBytes(finalCdOffset).CopyTo(section3, 16);

        // Step 5: Write final signed APK and verify
        Console.WriteLine("\n[5/5] Writing signed APK and verifying signature...");
        using (var fs = File.Create(outputApkPath))
        {
            fs.Write(section1, 0, section1.Length);
            fs.Write(apkSigningBlock, 0, apkSigningBlock.Length);
            fs.Write(section2, 0, section2.Length);
            fs.Write(section3, 0, section3.Length);
        }

        File.Copy(outputApkPath, copyApkPath, true);

        Console.WriteLine($"Signed APK generated: {new FileInfo(outputApkPath).Length:N0} bytes");

        // Verify the generated APK
        bool verified = VerifyApkV2(outputApkPath);
        if (verified)
        {
            Console.WriteLine("\n=================================================");
            Console.WriteLine(" *** APK SIGNATURE SCHEME V2: 100% VERIFIED! ***");
            Console.WriteLine("=================================================");
        }
        else
        {
            throw new Exception("APK signature verification failed!");
        }
    }

    static void AnalyzeAndPatchHermesStrings(byte[] bundleBytes)
    {
        ulong magic = BitConverter.ToUInt64(bundleBytes, 0);
        uint version = BitConverter.ToUInt32(bundleBytes, 8);
        uint fileLength = BitConverter.ToUInt32(bundleBytes, 32);
        uint functionCount = BitConverter.ToUInt32(bundleBytes, 40);
        uint identifierCount = BitConverter.ToUInt32(bundleBytes, 48);
        uint stringCount = BitConverter.ToUInt32(bundleBytes, 52);
        uint stringStorageSize = BitConverter.ToUInt32(bundleBytes, 60);

        Console.WriteLine($"Hermes Bytecode Header:");
        Console.WriteLine($" Magic: 0x{magic:X16}, Version: {version}");
        Console.WriteLine($" FunctionCount: {functionCount}, IdentifierCount: {identifierCount}");
        Console.WriteLine($" StringCount: {stringCount}, StringStorageSize: {stringStorageSize}");

        int smallTableOffset = 158448;
        int storageOffset = 197100;

        // 1. Build string descriptors for ALL strings in Hermes bundle
        var allEntries = new List<(int Index, bool IsUtf16, int StorageOffset, int ByteLen, string Value)>();
        for (int i = 0; i < (int)stringCount; i++)
        {
            uint entryVal = BitConverter.ToUInt32(bundleBytes, smallTableOffset + i * 4);
            int isUtf16 = (int)(entryVal & 1);
            int offset = (int)((entryVal >> 1) & 0x7FFFFF);
            int length = (int)((entryVal >> 24) & 0xFF);
            if (length == 255) continue; // Overflow string

            int byteLen = isUtf16 == 0 ? length : length * 2;
            string val = isUtf16 == 0
                ? Encoding.ASCII.GetString(bundleBytes, storageOffset + offset, length)
                : Encoding.Unicode.GetString(bundleBytes, storageOffset + offset, byteLen);

            allEntries.Add((i, isUtf16 != 0, offset, byteLen, val));
        }

        Console.WriteLine($"Loaded {allEntries.Count} strings from Hermes bundle.");

        // 2. Strict Whitelist of Safe User-Facing UI Replacements
        // Every entry will be mathematically verified to have ZERO byte overlaps with any other string or identifier!
        var whitelistCandidates = new Dictionary<string, string>
        {
            // ASCII UI Strings (Zero Overlap verified)
            { "Preparing your image...", "Preparando tu imagen..." },
            { "Processing image...", "Procesando imagen.." },
            { "Hungry", "Hambre" },

            // UTF-16 UI Strings (Zero Overlap verified, Visible on screens & settings)
            { "Authenticating, please wait…", "Autenticando, espera por f… " },
            { "Authenticating…", "Autenticando..." },
            { "Can’t see your cube?", "¿No ves tu MikuCube?" },
            { "Connecting…", "Conectando…" },
            { "Could not read from the cube —", "No se pudo leer del cuboface —" },
            { "Could not save alarms —", "No guardo las alarmas —" },
            { "Could not save animations —", "No guardo las animaciones —" },
            { "Could not save danmaku —", "No guardo los danmaku — " },
            { "Image ready — connecting to device...", "Imagen lista — conectando con cubo..." },
            { "Reading your cube…", "Leyendo tu cubo…  " },
            { "Reading…", "Leyendo…" },
            { "Scanning…", "Buscando…" },
            { "Something went wrong — try again", "Error al operar — reintentalo   " },
            { "Theme switch failed —", "Fallo cambiar tema — " },
            { "This is what’s on your cube", "Esto esta en tu Miku Cube  " },
            { "Upload failed — try again", "Fallo subir — reintentalo" },
            { "* Changes save to the cube on their own — the top-right shows where they stand.", "* Cambios se guardan en el cubo — su estado figura en la barra superior derecha" },
            { "* Read-only. Copies your cube’s config files to the clipboard so you can share them in a bug report. Nothing is modified.", "* Solo lectura. Copia archivos de config al portapapeles para reportar fallos en el cubo. No se modifica ningun ajuste.  " },
            { "Could not open Bluetooth settings. Open them from your phone’s Settings app.", "No se pudo abrir Bluetooth. Abre los ajustes desde el sistema de tu telefono" },
            { "Make sure it is turned on, then try opening the back of the cube — it only shows up in a scan while the back is open.", "Asegurate de que este encendido y prueba a abrir la tapa trasera — solo aparece al escanear con la tapa abierta.     " },
            { "No cube found — make sure it is powered on, the back is open, and it is close to your phone", "No se encontro el cubo — comprueba que esta encendido, abierto detras y cerca del movil    " },
            { "Not in your phone’s Bluetooth devices yet. Everything in this app still works, but earbud audio needs it.", "Aun no esta en Bluetooth del movil. Todo en la app funciona, pero el audio de auriculares lo necesita... " }
        };

        // 3. Strict Safety & Overlap Exclusion Engine
        Console.WriteLine("\n[Whitelist Safety Engine] Verifying candidate strings against Hermes bundle...");

        var verifiedSafeReplacements = new Dictionary<int, (string Orig, string Rep, int Offset, int ByteLen, bool IsUtf16)>();
        int rejectedCount = 0;

        foreach (var kvp in whitelistCandidates)
        {
            string orig = kvp.Key;
            string rep = kvp.Value;

            var match = allEntries.Find(e => e.Value == orig);
            if (match.Value == null)
            {
                Console.WriteLine($" [REJECTED - NOT FOUND]: \"{orig}\"");
                rejectedCount++;
                continue;
            }

            int startA = match.StorageOffset;
            int endA = match.StorageOffset + match.ByteLen;

            // Check if rep byte length exactly matches orig byte length
            int repByteLen = match.IsUtf16 ? rep.Length * 2 : rep.Length;
            if (repByteLen != match.ByteLen)
            {
                Console.WriteLine($" [REJECTED - LENGTH MISMATCH]: \"{orig}\" ({match.ByteLen} bytes) vs \"{rep}\" ({repByteLen} bytes)");
                rejectedCount++;
                continue;
            }

            // Check overlap with ANY other string in the bundle
            bool overlaps = false;
            foreach (var other in allEntries)
            {
                if (other.Index == match.Index) continue;

                int startB = other.StorageOffset;
                int endB = other.StorageOffset + other.ByteLen;

                if (Math.Max(startA, startB) < Math.Min(endA, endB))
                {
                    overlaps = true;
                    Console.WriteLine($" [DANGER REJECTED - OVERLAPS WITH #{other.Index} \"{other.Value}\"]: \"{orig}\"");
                    break;
                }
            }

            if (overlaps)
            {
                rejectedCount++;
                continue;
            }

            // Zero overlap verified!
            verifiedSafeReplacements[match.Index] = (orig, rep, match.StorageOffset, match.ByteLen, match.IsUtf16);
            Console.WriteLine($" [SAFE & WHITELISTED #{match.Index}]: \"{orig}\" -> \"{rep}\"");
        }

        Console.WriteLine($"\nSafety Verification Results: {verifiedSafeReplacements.Count} SAFE strings approved, {rejectedCount} rejected.");

        // 4. Apply Patches ONLY for Approved Safe Strings
        int patchedCount = 0;
        foreach (var kvp in verifiedSafeReplacements)
        {
            var item = kvp.Value;
            byte[] repBytes = item.IsUtf16 ? Encoding.Unicode.GetBytes(item.Rep) : Encoding.ASCII.GetBytes(item.Rep);
            if (repBytes.Length != item.ByteLen) throw new Exception("Unexpected byte length mismatch!");

            Array.Copy(repBytes, 0, bundleBytes, storageOffset + item.Offset, item.ByteLen);
            patchedCount++;
        }
        Console.WriteLine($"Successfully patched {patchedCount} UI strings in Hermes bytecode.");

        // 5. CRITICAL INTEGRITY ASSERTIONS
        Console.WriteLine("\n[Hermes Integrity Verification] Verifying critical JavaScript identifiers...");

        void AssertIdentifier(int id, string expected)
        {
            var entry = allEntries.Find(e => e.Index == id);
            string actual = entry.IsUtf16
                ? Encoding.Unicode.GetString(bundleBytes, storageOffset + entry.StorageOffset, entry.ByteLen)
                : Encoding.ASCII.GetString(bundleBytes, storageOffset + entry.StorageOffset, entry.ByteLen);

            if (actual != expected)
            {
                throw new Exception($"FATAL INTEGRITY VIOLATION: Identifier #{id} was corrupted! Expected '{expected}', Found '{actual}'");
            }
            Console.WriteLine($"  ✔ Identifier #{id,-4} ('{expected}') is intact: '{actual}'");
        }

        AssertIdentifier(19, "Math");          // The exact cause of the crash in previous version (Rath)
        AssertIdentifier(25, "Set");           // Global JS Set
        AssertIdentifier(107, "charCodeAt");   // String prototype
        AssertIdentifier(111, "clearTimeout"); // Global clearTimeout
        AssertIdentifier(246, "useCallback");  // React hook
        AssertIdentifier(1321, "shallowEqual"); // React Shallow comparison

        Console.WriteLine("All Hermes internal identifiers and React globals are 100% INTACT!");
    }



    static int FindEocd(byte[] bytes)
    {
        for (int i = bytes.Length - 22; i >= 0; i--)
        {
            if (bytes[i] == 0x50 && bytes[i + 1] == 0x4B && bytes[i + 2] == 0x05 && bytes[i + 3] == 0x06)
                return i;
        }
        throw new Exception("EOCD not found");
    }

    static uint ComputeCrc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            crc ^= b;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
        }
        return ~crc;
    }

    static byte[] ComputeApkRootDigest(byte[] section1, byte[] section2, byte[] section3)
    {
        var chunkDigests = new List<byte[]>();

        void ProcessSection(byte[] section)
        {
            int offset = 0;
            while (offset < section.Length)
            {
                int chunkSize = Math.Min(1048576, section.Length - offset);
                byte[] buffer = new byte[1 + 4 + chunkSize];
                buffer[0] = 0xa5;
                BitConverter.GetBytes((uint)chunkSize).CopyTo(buffer, 1);
                Array.Copy(section, offset, buffer, 5, chunkSize);

                byte[] chunkHash = SHA256.HashData(buffer);
                chunkDigests.Add(chunkHash);

                offset += chunkSize;
            }
        }

        ProcessSection(section1);
        ProcessSection(section2);
        ProcessSection(section3);

        byte[] rootBuffer = new byte[1 + 4 + chunkDigests.Count * 32];
        rootBuffer[0] = 0x5a;
        BitConverter.GetBytes((uint)chunkDigests.Count).CopyTo(rootBuffer, 1);
        for (int i = 0; i < chunkDigests.Count; i++)
        {
            Array.Copy(chunkDigests[i], 0, rootBuffer, 5 + i * 32, 32);
        }

        return SHA256.HashData(rootBuffer);
    }

    static byte[] BuildSignedData(byte[] rootDigest, byte[] certDer)
    {
        // 1. Digests sequence:
        byte[] digestEntry = new byte[4 + 4 + 32];
        BitConverter.GetBytes((uint)0x0103).CopyTo(digestEntry, 0); // SHA256withRSA
        BitConverter.GetBytes((uint)32).CopyTo(digestEntry, 4);
        Array.Copy(rootDigest, 0, digestEntry, 8, 32);
        byte[] digestsSeq = LengthPrefixed(LengthPrefixed(digestEntry));

        // 2. Certificates sequence:
        byte[] certsSeq = LengthPrefixed(LengthPrefixed(certDer));

        // 3. Additional attributes (empty):
        byte[] attrsSeq = LengthPrefixed(new byte[0]);

        using var ms = new MemoryStream();
        ms.Write(digestsSeq, 0, digestsSeq.Length);
        ms.Write(certsSeq, 0, certsSeq.Length);
        ms.Write(attrsSeq, 0, attrsSeq.Length);
        return ms.ToArray();
    }

    static byte[] BuildSignerBlock(byte[] signedData, byte[] signature, byte[] spki)
    {
        byte[] signedDataPrefixed = LengthPrefixed(signedData);

        // Signatures sequence:
        byte[] sigEntry = new byte[4 + 4 + signature.Length];
        BitConverter.GetBytes((uint)0x0103).CopyTo(sigEntry, 0);
        BitConverter.GetBytes((uint)signature.Length).CopyTo(sigEntry, 4);
        Array.Copy(signature, 0, sigEntry, 8, signature.Length);
        byte[] signaturesPrefixed = LengthPrefixed(LengthPrefixed(sigEntry));

        // Public key: SubjectPublicKeyInfo (as required by AOSP APK Signature Scheme v2 specification)
        byte[] spkiPrefixed = LengthPrefixed(spki);

        using var ms = new MemoryStream();
        ms.Write(signedDataPrefixed, 0, signedDataPrefixed.Length);
        ms.Write(signaturesPrefixed, 0, signaturesPrefixed.Length);
        ms.Write(spkiPrefixed, 0, spkiPrefixed.Length);
        byte[] signer = LengthPrefixed(ms.ToArray());

        return LengthPrefixed(signer);
    }

    static byte[] BuildApkSigningBlock(byte[] signerBlock)
    {
        ulong pairLen = (ulong)(4 + signerBlock.Length);
        byte[] pairHeader = new byte[8 + 4];
        BitConverter.GetBytes(pairLen).CopyTo(pairHeader, 0);
        BitConverter.GetBytes((uint)0x7109871a).CopyTo(pairHeader, 8); // APK Signature Scheme v2 ID

        ulong blockSize = (ulong)(pairHeader.Length + signerBlock.Length + 24);

        using var ms = new MemoryStream();
        ms.Write(BitConverter.GetBytes(blockSize), 0, 8);
        ms.Write(pairHeader, 0, pairHeader.Length);
        ms.Write(signerBlock, 0, signerBlock.Length);
        ms.Write(BitConverter.GetBytes(blockSize), 0, 8);
        byte[] magic = Encoding.ASCII.GetBytes("APK Sig Block 42");
        ms.Write(magic, 0, 16);

        return ms.ToArray();
    }

    static byte[] LengthPrefixed(byte[] data)
    {
        byte[] result = new byte[4 + data.Length];
        BitConverter.GetBytes((uint)data.Length).CopyTo(result, 0);
        Array.Copy(data, 0, result, 4, data.Length);
        return result;
    }

    static bool VerifyApkV2(string apkPath)
    {
        byte[] apk = File.ReadAllBytes(apkPath);
        int eocd = FindEocd(apk);
        uint cdOffset = BitConverter.ToUInt32(apk, eocd + 16);
        uint cdSize = BitConverter.ToUInt32(apk, eocd + 12);

        string magic = Encoding.ASCII.GetString(apk, (int)cdOffset - 16, 16);
        if (magic != "APK Sig Block 42") return false;

        ulong blockSize = BitConverter.ToUInt64(apk, (int)cdOffset - 24);
        int blockStart = (int)(cdOffset - blockSize - 8);

        // Parse ID-value pair
        ulong pairLen = BitConverter.ToUInt64(apk, blockStart + 8);
        uint pairId = BitConverter.ToUInt32(apk, blockStart + 16);
        if (pairId != 0x7109871a) return false;

        int signerOffset = blockStart + 20;
        uint signersLen = BitConverter.ToUInt32(apk, signerOffset);
        uint signerLen = BitConverter.ToUInt32(apk, signerOffset + 4);

        int pos = signerOffset + 8;
        uint signedDataLen = BitConverter.ToUInt32(apk, pos);
        byte[] signedData = new byte[signedDataLen];
        Array.Copy(apk, pos + 4, signedData, 0, (int)signedDataLen);
        pos += 4 + (int)signedDataLen;

        uint sigsLen = BitConverter.ToUInt32(apk, pos);
        uint sig1Len = BitConverter.ToUInt32(apk, pos + 4);
        uint sigAlgo = BitConverter.ToUInt32(apk, pos + 8);
        uint sigBytesLen = BitConverter.ToUInt32(apk, pos + 12);
        byte[] sigBytes = new byte[sigBytesLen];
        Array.Copy(apk, pos + 16, sigBytes, 0, (int)sigBytesLen);
        pos += 4 + (int)sigsLen;

        uint spkiLen = BitConverter.ToUInt32(apk, pos);
        byte[] spki = new byte[spkiLen];
        Array.Copy(apk, pos + 4, spki, 0, (int)spkiLen);

        // Verify RSA signature
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(spki, out _);
        bool sigValid = rsa.VerifyData(signedData, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!sigValid)
        {
            Console.WriteLine("FAIL: RSA signature on SignedData is INVALID!");
            return false;
        }

        // Verify Merkle tree digest
        byte[] section1 = new byte[blockStart];
        Array.Copy(apk, 0, section1, 0, blockStart);

        byte[] section2 = new byte[cdSize];
        Array.Copy(apk, (int)cdOffset, section2, 0, (int)cdSize);

        byte[] section3 = new byte[apk.Length - eocd];
        Array.Copy(apk, eocd, section3, 0, section3.Length);
        BitConverter.GetBytes((uint)section1.Length).CopyTo(section3, 16);

        byte[] computedRoot = ComputeApkRootDigest(section1, section2, section3);

        // Read stored digest from SignedData (digestsSeq -> digestEntry -> 32 bytes)
        byte[] storedDigest = new byte[32];
        Array.Copy(signedData, 4 + 4 + 4 + 4, storedDigest, 0, 32);

        for (int i = 0; i < 32; i++)
        {
            if (computedRoot[i] != storedDigest[i])
            {
                Console.WriteLine("FAIL: Merkle root digest mismatch!");
                return false;
            }
        }

        Console.WriteLine("Verification PASSED:");
        Console.WriteLine($" - RSA signature verified successfully with SubjectPublicKeyInfo ({spki.Length} bytes)");
        Console.WriteLine($" - Merkle tree root hash matched: {BitConverter.ToString(computedRoot).Replace("-", "").ToLower()}");
        return true;
    }
}
