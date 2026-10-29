using System;
using System.IO; // MemoryStream, BinaryWriter, BinaryReader
// Un tipo propio que viaja por la red. Photon no sabe enviarlo:
// le enseñamos a convertirlo en bytes (serializar) y a reconstruirlo (deserializar).
public class MensajeChat
{
    public string Texto;
    public string Hora; // «HH:mm» del que lo envía
    public bool Importante;
    // Objeto → bytes. Photon llama a este método al enviar.
    public static byte[] Serializar(object objeto)
    {
        MensajeChat m = (MensajeChat)objeto;
        using (MemoryStream memoria = new MemoryStream())
        using (BinaryWriter escritor = new BinaryWriter(memoria))
        {
            escritor.Write(m.Texto);
            escritor.Write(m.Hora);
            escritor.Write(m.Importante);
            escritor.Flush();
            return memoria.ToArray();
        }
    }
    // Bytes → objeto. Photon llama a este método al recibir.
    // Hay que leer en el MISMO orden en que se escribió.
    public static object Deserializar(byte[] datos)
    {
        using (MemoryStream memoria = new MemoryStream(datos))
        using (BinaryReader lector = new BinaryReader(memoria))
        {
            MensajeChat m = new MensajeChat();
            m.Texto = lector.ReadString();
            m.Hora = lector.ReadString();
            m.Importante = lector.ReadBoolean();
            return m;
        }
    }
}
