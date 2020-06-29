﻿using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using static MainController;
using static Assets.Game.Scripts.TherapyData;
using System.Text;

public class QuestFileManager
{
    #region Members
    private static byte[] m_key = new byte[]
    {
        24, 72, 55, 196, 120, 222, 187, 169, 9, 41, 211, 86, 252, 222, 159, 170,
        193, 27, 247, 104, 14, 236, 129, 205, 61, 24, 64, 119, 32, 197, 244, 144,
    };
    private static byte[] m_iv = new byte[] { 62, 172, 144, 170, 175, 27, 31, 183, 96, 240, 186, 146, 209, 109, 84, 143 };

    #endregion

    #region Public Methods

    public static void InitialPaths()
    {
        if (!Directory.Exists(PinchConstants.SystemDirectoryPath)) Directory.CreateDirectory(PinchConstants.SystemDirectoryPath);
        if (!Directory.Exists(PinchConstants.TherapistsDirectoryPath)) Directory.CreateDirectory(PinchConstants.TherapistsDirectoryPath);
        if (!Directory.Exists(PinchConstants.PatientsDirectoryPath)) Directory.CreateDirectory(PinchConstants.PatientsDirectoryPath);
    }

    /// <summary>
    /// This function will save the therapist as an encrypted file in "filePath".
    /// Will return false if file already exist.
    /// </summary>
    /// <param name="therapist"></param>
    /// <param name="filePath"> the path with name of file</param>
    public static bool SaveTherapistToFile(Therapist therapist, string filePath)
    {
        if (File.Exists(filePath)) return false;
        byte[] encrypted = Encrypt(JsonUtility.ToJson(therapist, true));
        File.WriteAllBytes(filePath, encrypted);
        return true;
    }

    /// <summary>
    /// Will return a therapist if exist, else null.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Therapist GetTherapistFromFile(string filePath)
    {
        Therapist therapist = null;
        if (File.Exists(filePath))
        {
            byte[] encrypted = File.ReadAllBytes(filePath);
            string jsonString = Decrypt(encrypted);
            therapist = JsonUtility.FromJson<Therapist>(jsonString);
        }

        return therapist;
    }

    /// <summary>
    /// This function will update an existing therapist file which will save as an encrypted file in "filePath".
    /// Will return false if file is not exist.
    /// </summary>
    /// <param name="patient"> the updated therapist</param>
    /// <param name="filePath"> the path with name of file</param>
    public static bool UpdateTherapistOnFile(Therapist therapist, string filePath)
    {
        if (!File.Exists(filePath)) return false;
        byte[] encrypted = Encrypt(JsonUtility.ToJson(therapist, true));
        File.WriteAllBytes(filePath, encrypted);
        return true;
    }


    /// <summary>
    /// This function will save a new patient as an encrypted file in "filePath".
    /// Will return false if file already exist.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="filePath"> the path with name of file</param>
    public static bool SaveNewPatientToFile(Patient patient, string filePath)
    {
        if (File.Exists(filePath)) return false;
        byte[] encrypted = Encrypt(JsonUtility.ToJson(patient, true));
        File.WriteAllBytes(filePath, encrypted);
        return true;
    }

    /// <summary>
    /// This function will update an existing patient file which will save as an encrypted file in "filePath".
    /// Will return false if file is not exist.
    /// </summary>
    /// <param name="patient"> the updated patient</param>
    /// <param name="filePath"> the path with name of file</param>
    public static bool UpdatePatientOnFile(Patient patient, string filePath)
    {
        if (!File.Exists(filePath)) return false;
        byte[] encrypted = Encrypt(JsonUtility.ToJson(patient, true));
        File.WriteAllBytes(filePath, encrypted);
        return true;
    }

    /// <summary>
    /// Will return a patient if exist, else null.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Patient GetPatientFromFile(string filePath)
    {
        Patient patient = null;
        if (File.Exists(filePath))
        {
            byte[] encrypted = File.ReadAllBytes(filePath);
            string jsonString = Decrypt(encrypted);
            patient = JsonUtility.FromJson<Patient>(jsonString);
        }

        return patient;
    }
    #endregion

    #region Crypto Methods
    private static byte[] Encrypt(string plainText)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            Debug.LogError("Null plainText to encrypt");
        if (m_key == null || m_key.Length <= 0)
            Debug.LogError("Null encryption key");
        if (m_iv == null || m_iv.Length <= 0)
            Debug.LogError("Null encryption IV");

        byte[] encrypted;

        Aes aesAlg = Aes.Create();

        aesAlg.Key = m_key;
        aesAlg.IV = m_iv;

        // Create an encryptor to perform the stream transform.
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for encryption.
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }
        }


        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    private static string Decrypt(byte[] cipherText)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            Debug.LogError("Null plainText to decrypt");
        if (m_key == null || m_key.Length <= 0)
            Debug.LogError("Null encryption key");
        if (m_iv == null || m_iv.Length <= 0)
            Debug.LogError("Null encryption IV");
        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        Aes aesAlg = Aes.Create();

        aesAlg.Key = m_key;
        aesAlg.IV = m_iv;

        // Create a decryptor to perform the stream transform.
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for decryption.
        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {

                    // Read the decrypted bytes from the decrypting stream
                    // and place them in a string.
                    plaintext = srDecrypt.ReadToEnd();
                }
            }
        }

        return plaintext;
    }
    #endregion
}


//public static bool WriteTreatmentToTempFile(TreatmentPlan treatmentPlan)
//{
//    try
//    {
//        byte[] encrypted = Encrypt(JsonUtility.ToJson(treatmentPlan));
//        File.WriteAllBytes(PinchConstants.TempFileForCurrentTreatment, encrypted);
//        return true;
//    }
//    catch (Exception e)
//    {
//        PrintToLog(e.ToString(), MainController.LogType.Error);
//        return false;
//    }
//}

//public static TreatmentPlan GetTreatmentFromTempFile()
//{
//    try
//    {
//        byte[] encrypted = File.ReadAllBytes(PinchConstants.TempFileForCurrentTreatment);
//        string jsonString = Decrypt(encrypted);
//        return JsonUtility.FromJson<TreatmentPlan>(jsonString);
//    }
//    catch (Exception e)
//    {
//        PrintToLog(e.ToString(), MainController.LogType.Error);
//        return null;
//    }
//}