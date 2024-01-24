using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static PalDecompresses.Global;
using static PalDecompresses.PalUtil;

namespace PalDecompresses
{
    public unsafe class FileUtil
    {
        public static int
        PAL_Read(
            byte[]* buf,
            int iCount,
            int iOffset,
            FileStream fsFile
        )
        {
            return fsFile.Read(*buf, iOffset, iCount);
        }

        public static long
        PAL_Seek(
            long lOffset,
            SeekOrigin soOrigin,
            FileStream fsFile
        )
        {
            return fsFile.Seek(lOffset, soOrigin);
        }

        public static int
        PAL_Fread(
            byte[]* buf,
            int iCount,
            int iOffset,
            FileStream fsFile
        )
        {
            if (PAL_Read(buf, iCount, iOffset, fsFile) < iCount) return -1;
            else return 0;
        }

        public static FileStream
        UTIL_OpenRequiredFile(
           string strFileName
        )
        /*++
          Purpose:

            Open a required file. If fails, quit the program.

          Parameters:

            [IN]  lpszFileName - file name to open.

          Return value:

            Pointer to the file.

        --*/
        {
            return UTIL_OpenRequiredFileForMode(strFileName, FileMode.Open, FileAccess.Read);
        }

        public static FileStream
        UTIL_OpenRequiredFileForMode(
           string strFileName,
           FileMode fmMode,
           FileAccess faAccess
        )
        /*++
          Purpose:

            Open a required file. If fails, quit the program.

          Parameters:

            [IN]  lpszFileName - file name to open.
            [IN]  szMode - file open mode.

          Return value:

            Pointer to the file.

        --*/
        {
            FileStream? fsFile = null;
            string strError = "Failed to open file a.txt" + strFileName + "!\n";
            try
            {
                fsFile = new FileStream(strFileName, fmMode, faAccess);

                PAL_Failed(fsFile == null, strError);
            }
            catch (FileNotFoundException)
            {
                PAL_Failed(TRUE, strError);
                // throw new Exception(strError);
            }
            // finally
            // {
            //     fsFile?.Close();
            // }

            return fsFile;
        }

        public static void
        UTIL_CreateDirs(
            string strDirsPath
        )
        {
            if (!Directory.Exists(strDirsPath)) Directory.CreateDirectory(strDirsPath);
        }

        public static FileStream
        UTIL_CreateFile(
            string? strFilePath,
            string strFileName
        )
        /*++
          Purpose:

            Create a file. If fails, quit the program.

          Parameters:

            [IN]  lpszFileName - file name to open.

          Return value:

            Pointer to the file.

        --*/
        {
            FileStream fsFile = null;
            string strFliePathName = ((strFilePath != null) ? strFilePath : "") + strFileName;

            string strError = $"Failed to open file {strFileName} !\n";

            UTIL_CreateDirs(strFilePath);

            try
            {
                fsFile = new FileStream(strFliePathName, FileMode.Create);

                PAL_Failed(fsFile == null, strError);
            }
            catch (FileNotFoundException)
            {
                PAL_Failed(TRUE, strError);
                // throw new Exception(strError);
            }

            return fsFile;
        }

        public static void
        UTIL_BinaryWrite(
            string? stObjPath,
            string strObj,
            string strContent
        )
        {
            byte[] baBuf;

            using (FileStream fsSaveCSV = UTIL_CreateFile(strSaveDocPath + ((stObjPath != null) ? stObjPath + PathDSC : ""), strObj + strSaveDocExt))
            {
                using (BinaryWriter bwBinSave = new BinaryWriter(fsSaveCSV))
                {
                    //
                    // 注册Nuget包System.Text.Encoding.CodePages中的编码到.NET 6
                    //
                    UTIL_RegEncode();
                    Encoding gb2312 = Encoding.GetEncoding("gb2312");

                    baBuf = gb2312.GetBytes(strContent);
                    bwBinSave.Write(baBuf);
                }
            }
        }

        public static void
        UTIL_RegEncode()
        {
            if (!fIsRegEncode)
            {
                fIsRegEncode = TRUE;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
        }

        public static Encoding
        UTIL_GetEncode()
        {
            return Encoding.GetEncoding((!fIsWIN95) ? "big5" : "gb2312");
        }
    }
}
