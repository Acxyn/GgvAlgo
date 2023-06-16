using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxynAPI.Common
{
    public class Password
    {
        public string Encode(string password)
        {
            try
            {
                byte[] EncDataByte = new byte[password.Length];
                EncDataByte = System.Text.Encoding.UTF8.GetBytes(password);
                string EncryptedData = Convert.ToBase64String(EncDataByte);

                return EncryptedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Encode: " + ex.Message);
            }
        }

        public string Decode(string password)
        {
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(password);
                string decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);

                return decodedString;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Encode: " + ex.Message);
            }
        }
    }
}
