
# XOR Encryption and PowerShell Loader
## Descripción del Proyecto

Este proyecto implementa un cifrado basado en XOR con una clave dinámica y una opción de ofuscación personalizada. El objetivo es cifrar un archivo binario o ejecutable y generar un script PowerShell que descifre y ejecute el archivo en memoria. Este enfoque mejora la seguridad al evitar que el archivo descifrado se escriba en el disco, dificultando la ingeniería inversa y la detección estática.
Características

Cifrado XOR Dinámico: Cada byte del archivo se cifra utilizando una clave XOR que cambia dinámicamente en cada iteración.
Ofuscación Personalizada: Se reemplazan ciertos valores hexadecimales con símbolos personalizados (♘, ♟, ♜) para complicar la detección mediante análisis estático.
Generación de Scripts PowerShell: El script PowerShell generado descifra y ejecuta el archivo en memoria.
Verificación de Integridad: Se genera un hash SHA256 del archivo cifrado para verificar la integridad de los datos.
