/*
 $Id$
 */
/*
 * Last edited by Thuat
 * Last modified time: 27 March 2010
 * Changes:
 *  + Add in 2 stub methods: TotalCapacity and SpareCapacity of a drive
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.IO;

namespace OneSync.Files
{
    /// <summary>
    /// Contains methods to get information regarding a Drive.
    /// </summary>
    public static class DriveUtil
    {

        /// <summary>
        /// Provides access to information on a drive.
        /// </summary>
        public class DriveDetails
        {
            /// <summary>
            /// Model of drive e.g. FUJITSU MHW2 ATA Device
            /// </summary>
            public string Model { get; set; }

            /// <summary>
            /// Signature of drive e.g. 2327832874
            /// </summary>
            public string Signature { get; set; }

            /// <summary>
            /// Serial number of drive. e.g. 4h2j5jzxkcsdfasdf2
            /// </summary>
            public string SerialNumber { get; set; }

            /// <summary>
            /// Plug and Play device ID. Uniquely identify each drive??
            /// </summary>
            public string PNPDeviceID { get; set; }


            /// <summary>
            /// Volume serial number of a partition. e.g. 28213972
            /// Note that a hard disk can have several partition.
            /// </summary>
            public string VolumeSerialNumber { get; set; }
        }

        /// <summary>
        /// Determines whether a drive is removable or not.
        /// </summary>
        /// <param name="driveLetter">
        /// A valid drive drive letter.
        /// This can be either uppercase or lowercase, 'a' to 'z'.
        /// </param>
        /// <returns>true, if drive with specified drive letter is removable.</returns>
        public static bool IsRemovable(char driveLetter)
        {
            /*
            //"SELECT * FROM Win32_LogicalDisk WHERE Caption = '" + driveLetter + ":'"
            SelectQuery q = new SelectQuery("Win32_LogicalDisk", "Caption = '" + driveLetter + ":'");

            ManagementObjectSearcher s = new ManagementObjectSearcher(q);

            foreach (ManagementObject service in s.Get())
            {
                if (service["Description"].ToString().Equals("Local Fixed Disk"))
                    return false;
            }

            return true;
            */

            return new DriveInfo(driveLetter.ToString()).DriveType == DriveType.Removable;
        }


        
        

        /// <summary>
        /// Determines whether the filesystem of the drive is NTFS.
        /// </summary>
        /// <param name="driveLetter">
        /// A valid drive drive letter.
        /// This can be either uppercase or lowercase, 'a' to 'z'.
        /// </param>
        /// <returns>true, if the drive with specified drive letter is NTFS</returns>
        public static bool IsNTFS(char driveLetter)
        {
            DriveInfo drive = new DriveInfo(driveLetter.ToString());
            return drive.DriveFormat.Equals("NTFS");
        }

        /// <summary>
        /// Gets the details of drive with specified drive letter.
        /// </summary>
        /// <param name="driveLetter">
        /// A valid drive drive letter.
        /// This can be either uppercase or lowercase, 'a' to 'z'.
        /// </param>
        /// <returns>Drive details.</returns>
        public static DriveDetails GetDetails(char driveLetter)
        {
            return GetDetails(GetDeviceId(driveLetter), driveLetter);
        }

        /// <summary>
        /// Gets the total capacity of a drive with specific drive letter
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns>Total number of capacity</returns>
        public static long TotalCapacityInBytes(char driveLetter)
        {
            return new DriveInfo (driveLetter.ToString()).TotalSize;
        }

        /// <summary>
        /// Get the spare capacity of a drive with specified drive letter
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        public static long SpareCapacityInBytes(char driveLetter)
        {
            return new DriveInfo (driveLetter.ToString ()).TotalFreeSpace;
        }



        #region Private Methods

        /// <summary>
        /// Retrieves Device ID of a drive (e.g. PHYSICALDRIVE0)
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        private static string GetDeviceId(char driveLetter)
        {
            ManagementObjectSearcher s1 = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDiskToPartition");
            ManagementObjectSearcher s2 = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDriveToDiskPartition");

            string partition = "";
            string devID = "";

            // Extract partition number
            foreach (ManagementObject service in s1.Get())
            {
                if (service["Dependent"].ToString().Contains(driveLetter + ":"))
                {
                    int index = service["Antecedent"].ToString().LastIndexOf("=") + 1;
                    partition = service["Antecedent"].ToString().Substring(index);
                    break;
                }
            }

            // Get Device ID according to partition number
            foreach (ManagementObject service in s2.Get())
            {
                if (service["Dependent"].ToString().Contains(partition))
                {
                    int index = service["Antecedent"].ToString().LastIndexOf(@"\\") + 1;
                    devID = service["Antecedent"].ToString().Substring(index);

                    // Remove last quote character
                    devID = devID.Substring(0, devID.Length - 1);
                    break;
                }
            }

            return devID;
        }

        /// <summary>
        /// Get the details of drive with specified device ID.
        /// </summary>
        /// <param name="deviceId">Device ID. e.g. PHYSICALDRIVE0</param>
        /// <param name="driveLetter">
        /// A valid drive drive letter.
        /// This can be either uppercase or lowercase, 'a' to 'z'.
        /// </param>
        /// <returns>Drive Details</returns>
        private static DriveDetails GetDetails(string deviceId, char driveLetter)
        {
            DriveDetails details = new DriveDetails();

            SelectQuery q1 = new SelectQuery("SELECT * FROM Win32_DiskDrive");
            SelectQuery q2 = new SelectQuery("SELECT * FROM Win32_LogicalDisk");

            ManagementObjectSearcher s1 = new ManagementObjectSearcher(q1);

            foreach (ManagementObject service in s1.Get())
            {
                if (service["DeviceID"].ToString().Contains(deviceId))
                {
                    details.Model = service["Model"].ToString();
                    details.Signature = service["Signature"].ToString();
                    details.SerialNumber = service["SerialNumber"].ToString();
                    details.PNPDeviceID = service["PNPDeviceID"].ToString();
                    break;
                }
            }

            ManagementObjectSearcher s2 = new ManagementObjectSearcher(q2);

            foreach (ManagementObject service in s2.Get())
            {
                if (service["DeviceID"].ToString().Contains(driveLetter.ToString()))
                {
                    details.VolumeSerialNumber = service["VolumeSerialNumber"].ToString();
                    break;
                }
            }

            return details;
        }

        #endregion


    }
}
