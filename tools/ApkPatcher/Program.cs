using System;
using System.IO;
using System.IO.Compression;
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
        string originalApkPath = Path.Combine(projectRoot, "MikuCube-v1.0.2.apk");
        string tempApkPath = Path.Combine(Path.GetTempPath(), "mikucube_patched_unsigned.apk");
        string outputApkPath = Path.Combine(projectRoot, "MikuCube-v1.0.2-ES.apk");

        Console.WriteLine("Step 1: Reading original APK and bundle...");
        byte[] bundleBytes = null;
        using (var zip = ZipFile.OpenRead(originalApkPath))
        {
            var bundleEntry = zip.GetEntry("assets/index.android.bundle");
            if (bundleEntry == null) throw new Exception("assets/index.android.bundle not found!");
            using (var stream = bundleEntry.Open())
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bundleBytes = ms.ToArray();
            }
        }

        Console.WriteLine($"Original bundle size: {bundleBytes.Length} bytes");

        Console.WriteLine("Step 2: Patching strings in Hermes bytecode...");
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

        // SmallStringTable begins at 158448, storage starts at 197100
        int storageOffset = 197100;
        int storageSize = 157872;
        int smallTableOffset = 158448;
        int stringCount = 9643;

        int patchedCount = 0;
        for (int i = 0; i < stringCount; i++)
        {
            uint entryVal = BitConverter.ToUInt32(bundleBytes, smallTableOffset + i * 4);
            int isUtf16 = (int)(entryVal & 1);
            int offset = (int)((entryVal >> 1) & 0x7FFFFF);
            int length = (int)((entryVal >> 24) & 0xFF);
            if (length == 255) continue; // skip overflow

            if (isUtf16 == 0)
            {
                string original = Encoding.ASCII.GetString(bundleBytes, storageOffset + offset, length);
                if (replacements.TryGetValue(original, out string rep))
                {
                    if (rep.Length != length)
                    {
                        Console.WriteLine($"WARNING: length mismatch for '{original}': {length} vs {rep.Length}");
                        continue;
                    }
                    byte[] repBytes = Encoding.ASCII.GetBytes(rep);
                    Array.Copy(repBytes, 0, bundleBytes, storageOffset + offset, length);
                    patchedCount++;
                }
            }
            else
            {
                string original = Encoding.Unicode.GetString(bundleBytes, storageOffset + offset, length * 2);
                if (utf16Replacements.TryGetValue(original, out string rep))
                {
                    if (rep.Length != length)
                    {
                        Console.WriteLine($"WARNING: length mismatch for UTF16 '{original}': {length} vs {rep.Length}");
                        continue;
                    }
                    byte[] repBytes = Encoding.Unicode.GetBytes(rep);
                    Array.Copy(repBytes, 0, bundleBytes, storageOffset + offset, length * 2);
                    patchedCount++;
                }
            }
        }

        Console.WriteLine($"Patched {patchedCount} UI strings in Hermes bytecode!");

        Console.WriteLine("Step 3: Creating unsigned APK with patched bundle...");
        if (File.Exists(tempApkPath)) File.Delete(tempApkPath);
        File.Copy(originalApkPath, tempApkPath, true);

        using (var zip = ZipFile.Open(tempApkPath, ZipArchiveMode.Update))
        {
            var oldEntry = zip.GetEntry("assets/index.android.bundle");
            oldEntry.Delete();

            var newEntry = zip.CreateEntry("assets/index.android.bundle", CompressionLevel.Optimal);
            using (var stream = newEntry.Open())
            {
                stream.Write(bundleBytes, 0, bundleBytes.Length);
            }
        }

        Console.WriteLine("Step 4: Signing APK with APK Signature Scheme v2...");
        SignApk(tempApkPath, outputApkPath);
        File.Delete(tempApkPath);

        Console.WriteLine($"SUCCESS! Signed APK created at: {outputApkPath}");
        Console.WriteLine($"File size: {new FileInfo(outputApkPath).Length:N0} bytes");
    }

    static void SignApk(string inputApk, string outputApk)
    {
        byte[] apkBytes = File.ReadAllBytes(inputApk);

        // Find EOCD
        int eocdOffset = -1;
        for (int i = apkBytes.Length - 22; i >= 0; i--)
        {
            if (apkBytes[i] == 0x50 && apkBytes[i + 1] == 0x4B && apkBytes[i + 2] == 0x05 && apkBytes[i + 3] == 0x06)
            {
                eocdOffset = i;
                break;
            }
        }
        if (eocdOffset == -1) throw new Exception("EOCD not found");

        uint cdOffset = BitConverter.ToUInt32(apkBytes, eocdOffset + 16);
        uint cdSize = BitConverter.ToUInt32(apkBytes, eocdOffset + 12);

        // Check if there was an APK signing block before CD
        // In our fresh zip created by .NET ZipArchive, there is NO APK signing block before CD.
        // Section 1: 0 to cdOffset
        // Section 2: cdOffset to cdOffset + cdSize
        // Section 3: EOCD (with CD offset set to cdOffset)
        byte[] section1 = new byte[cdOffset];
        Array.Copy(apkBytes, 0, section1, 0, cdOffset);

        byte[] section2 = new byte[cdSize];
        Array.Copy(apkBytes, cdOffset, section2, 0, cdSize);

        byte[] section3 = new byte[apkBytes.Length - eocdOffset];
        Array.Copy(apkBytes, eocdOffset, section3, 0, section3.Length);

        // Compute Merkle tree digests
        byte[] rootDigest = ComputeApkRootDigest(section1, section2, section3);
        Console.WriteLine($"Computed root digest: {BitConverter.ToString(rootDigest).Replace("-", "").ToLower()}");

        // Generate RSA-2048 key and self-signed certificate
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=MikuCube-ESP, O=MikuCube-ESP", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(30));
        byte[] certDer = cert.RawData;

        // Construct SignedData
        byte[] signedData = BuildSignedData(rootDigest, certDer);

        // Sign SignedData with RSA-SHA256 (0x0103)
        byte[] signature = rsa.SignData(signedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Construct Signers Block
        byte[] signerBlock = BuildSignerBlock(signedData, signature, certDer);

        // Construct APK Signing Block
        byte[] apkSigningBlock = BuildApkSigningBlock(signerBlock);

        // Update Section 3 (EOCD): CD offset becomes section1.Length + apkSigningBlock.Length
        uint newCdOffset = (uint)(section1.Length + apkSigningBlock.Length);
        byte[] newCdOffsetBytes = BitConverter.GetBytes(newCdOffset);
        Array.Copy(newCdOffsetBytes, 0, section3, 16, 4);

        // Write final signed APK
        using (var fs = File.Create(outputApk))
        {
            fs.Write(section1, 0, section1.Length);
            fs.Write(apkSigningBlock, 0, apkSigningBlock.Length);
            fs.Write(section2, 0, section2.Length);
            fs.Write(section3, 0, section3.Length);
        }
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
                // Chunk digest = SHA256(0xa5 + uint32_le(chunkSize) + chunk)
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

        // Root digest = SHA256(0x5a + uint32_le(chunkDigests.Count) + all_chunk_digests)
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
        // 1. Digests:
        // digest entry: uint32 sigAlgo (0x0103), uint32 digestLen (32), byte[32] rootDigest
        byte[] digestEntry = new byte[4 + 4 + 32];
        BitConverter.GetBytes((uint)0x0103).CopyTo(digestEntry, 0);
        BitConverter.GetBytes((uint)32).CopyTo(digestEntry, 4);
        Array.Copy(rootDigest, 0, digestEntry, 8, 32);

        byte[] digestsSequence = LengthPrefixed(LengthPrefixed(digestEntry));

        // 2. Certificates:
        byte[] certsSequence = LengthPrefixed(LengthPrefixed(certDer));

        // 3. Additional attributes (empty):
        byte[] attrsSequence = LengthPrefixed(new byte[0]);

        // Concatenate the three sequences
        using var ms = new MemoryStream();
        ms.Write(digestsSequence, 0, digestsSequence.Length);
        ms.Write(certsSequence, 0, certsSequence.Length);
        ms.Write(attrsSequence, 0, attrsSequence.Length);
        return ms.ToArray();
    }

    static byte[] BuildSignerBlock(byte[] signedData, byte[] signature, byte[] certDer)
    {
        // SignedData prefixed with uint32 length
        byte[] signedDataPrefixed = LengthPrefixed(signedData);

        // Signatures:
        // sig entry: uint32 sigAlgo (0x0103), uint32 sigBytesLen, byte[] signature
        byte[] sigEntry = new byte[4 + 4 + signature.Length];
        BitConverter.GetBytes((uint)0x0103).CopyTo(sigEntry, 0);
        BitConverter.GetBytes((uint)signature.Length).CopyTo(sigEntry, 4);
        Array.Copy(signature, 0, sigEntry, 8, signature.Length);
        byte[] signaturesPrefixed = LengthPrefixed(LengthPrefixed(sigEntry));

        // Public key (certDer prefixed)
        byte[] certPrefixed = LengthPrefixed(certDer);

        // Signer = LengthPrefixed(signedDataPrefixed + signaturesPrefixed + certPrefixed)
        using var ms = new MemoryStream();
        ms.Write(signedDataPrefixed, 0, signedDataPrefixed.Length);
        ms.Write(signaturesPrefixed, 0, signaturesPrefixed.Length);
        ms.Write(certPrefixed, 0, certPrefixed.Length);
        byte[] signer = LengthPrefixed(ms.ToArray());

        // Signers block = LengthPrefixed(signer)
        return LengthPrefixed(signer);
    }

    static byte[] BuildApkSigningBlock(byte[] signerBlock)
    {
        // ID-value pair: uint64 pairLen, uint32 ID (0x7109871a), byte[] signerBlock
        // pairLen = 4 + signerBlock.Length
        ulong pairLen = (ulong)(4 + signerBlock.Length);
        byte[] pairHeader = new byte[8 + 4];
        BitConverter.GetBytes(pairLen).CopyTo(pairHeader, 0);
        BitConverter.GetBytes((uint)0x7109871a).CopyTo(pairHeader, 8);

        // Block size: 8 + pairHeader.Length - 8 + signerBlock.Length = 8 + 4 + signerBlock.Length = 12 + signerBlock.Length
        // Total block bytes:
        // uint64 blockSize
        // pairHeader
        // signerBlock
        // uint64 blockSize
        // 16 bytes magic "APK Sig Block 42"
        ulong totalBlockDataSize = 8 + (ulong)pairHeader.Length - 8 + (ulong)signerBlock.Length;
        // In APK Sig Block specification:
        // blockSize (uint64 at start and end) is the size of the block excluding the first uint64 blockSize field itself.
        // So blockSize = (totalBlockDataSize) + 8 (for the trailing blockSize) + 16 (for magic) = pairHeader.Length + signerBlock.Length + 24
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
}
