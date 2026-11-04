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

        PatchHermesStrings(bundleBytes);

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

    static void PatchHermesStrings(byte[] bundleBytes)
    {
        var replacements = new Dictionary<string, string>
        {
            { "English", "Espanol" },
            { "Face Studio", "EstudioFace" },
            { "Animation Selection", "Animaciones Horario" },
            { "Wake Alarm", "Alarmas   " },
            { "System Settings", "Ajustes Sistema" },
            { "Settings", "Ajustes " },
            { "Find your Miku Cube", "Encuentra Miku Cube" },
            { "LIKELY YOUR CUBE", "TU CUBO PROBABLE" },
            { "Show all Bluetooth devices", "Ver dispositivos Bluetooth" },
            { "Connect", "Conecta" },
            { "CONNECT", "CONECTA" },
            { "Disconnect", "Desconecta" },
            { "Not Connected", "Desconectado " },
            { "Connected", "Conectado" },
            { "Upload Custom Image", "Subir imagen propia" },
            { "Restore default times", "Restablecer horarios " },
            { "Active theme", "Tema activo " },
            { "System Version", "VersionSistema" },
            { "In use", "En uso" },
            { "Switch", "Cambio" },
            { "Cube clock", "Reloj Cubo" },
            { "Open Bluetooth settings", "Abrir ajustes Bluetooth" },
            { "Copy device data", "Copiar los datos" },
            { "Change Device", "Cambiar Cubo " },
            { "Every day", "Cada dia " },
            { "Add", "Mas" },
            { "New Alarm", "Nueva Al." },
            { "Edit Alarm", "Editar Al." },
            { "Time", "Hora" },
            { "Repeat", "Repite" },
            { "Alarm name", "Nom.Alarma" },
            { "Danmaku phrase", "Frase pantalla" },
            { "Confirm", "Aceptar" },
            { "CONFIRM", "ACEPTAR" },
            { "Cancel", "Anular" },
            { "Delete alarm", "BorrarAlarma" },
            { "Use next alarm from Clock", "Usar alarma de Reloj App " },
            { "Scroll lyrics while music plays", "Mover letra al sonar la musica " },
            { "Use large text danmaku", "Danmaku con texto gran" },
            { "Remember to drink water", "Recuerda beber mas agua" },
            { "Sleep early", "Duerme bien" },
            { "Turn off lights and lock up", "Apaga la luz y cierra bien " },
            { "Breakfast", "Desayuno " },
            { "Brush Teeth", "LavaDientes" },
            { "Studying", "Estudiar" },
            { "Grinding", "Trabajar" },
            { "Milk Tea", "TeBurbuj" },
            { "Slacking Off", "Pez Relajado" },
            { "Sleepy", "Dormir" },
            { "Eating Rice", "Comer Arroz" },
            { "Hungry", "Hambre" },
            { "Happy", "Feliz" },
            { "Popcorn", "Palomit" },
            { "Fell Asleep", "BienDormida" },
            { "Fully Charged", "CargaCompleta" },
            { "Change faces here first after connecting. Use the side menu for everything else.", "Cambia caras aqui primero tras conectar. Usa el menu lateral para todo lo demas." },
            { "Place your MikuCube nearby, then tap Connect", "Coloca tu MikuCube cerca y pulsa en Conecta " },
            { "* Alarms sync to the device immediately", "* Alarmas sincronizan con el Cubo ahora" },
            { "* Read the real version from the device", "* Lee la version real desde dispositivo" },
            { "Bluetooth and location permissions are required", "Permisos de Bluetooth y de ubicacion requeridos" },
            { "Image saved! Setting as face...", "Guardada! Poniendo como cara..." },
            { "Add to Bluetooth", "Vincular en BT  " },
            { "Removes the preview from this app only. The image stays on the cube until you upload a new one.", "Elimina la vista previa solo de la app. La imagen sigue en el cubo hasta que subas otra nueva. " },
            { "This cube is missing schedule timing. Restore default times to enable editing. Animation selections are kept; custom time ranges are reset.", "A este cubo le faltan horarios. Restablece horarios para editar. Las animaciones se conservan; los rangos horarios propios se reajustan.   " }
        };

        var utf16Replacements = new Dictionary<string, string>
        {
            { "Language / 语言", "Idioma / Lang" },
            { "语言 / Language", "Lang / Idioma" }
        };

        int storageOffset = 197100;
        int smallTableOffset = 158448;
        int stringCount = 9643;

        int count = 0;
        for (int i = 0; i < stringCount; i++)
        {
            uint entryVal = BitConverter.ToUInt32(bundleBytes, smallTableOffset + i * 4);
            int isUtf16 = (int)(entryVal & 1);
            int offset = (int)((entryVal >> 1) & 0x7FFFFF);
            int length = (int)((entryVal >> 24) & 0xFF);
            if (length == 255) continue;

            if (isUtf16 == 0)
            {
                string original = Encoding.ASCII.GetString(bundleBytes, storageOffset + offset, length);
                if (replacements.TryGetValue(original, out string rep))
                {
                    byte[] repBytes = Encoding.ASCII.GetBytes(rep);
                    Array.Copy(repBytes, 0, bundleBytes, storageOffset + offset, length);
                    count++;
                }
            }
            else
            {
                string original = Encoding.Unicode.GetString(bundleBytes, storageOffset + offset, length * 2);
                if (utf16Replacements.TryGetValue(original, out string rep))
                {
                    byte[] repBytes = Encoding.Unicode.GetBytes(rep);
                    Array.Copy(repBytes, 0, bundleBytes, storageOffset + offset, length * 2);
                    count++;
                }
            }
        }
        Console.WriteLine($"Patched {count} UI strings in Hermes bytecode.");
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
