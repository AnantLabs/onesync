using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OneSyncATD
{
    public class Utility
    {

        /// <summary>
        /// Creates a file with specified size at . Destination file will be overwritten if it already exists.
        /// </summary>
        /// <param name="destFilePath">Destination file path (can be relative).</param>
        /// <param name="size">Size of file to be created in bytes.</param>
        public static void CreateFile(string destFilePath, long size)
        {
            using (FileStream fs = File.Create(destFilePath))
            {
                fs.SetLength(size);
            }
        }

        public static void Serialize(object o, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Construct a BinaryFormatter and use it to serialize the data to the stream.
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, o);
                }
                catch (SerializationException ex)
                {
                    //MessageBox.Show("Failed to serialize. Reason: " + ex.Message);
                    throw;
                }
            }//end using
        }

        public static object Deserialize(string filePath)
        {
            try
            {
                Object o = null;

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    o = formatter.Deserialize(fs);
                }
                return o;
            }
            catch (SerializationException ex)
            {
                //MessageBox.Show("Failed to deserialize. Reason: " + ex.Message);
                throw;
            }
        }
    }
}
