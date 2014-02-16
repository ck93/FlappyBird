using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace FlappyBird
{
    class FTP
    {
        static public bool FTPAvailable()
        {
            try
            {
                FtpWebRequest ReqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://59.66.133.208/flappy bird/records.txt"));
                ReqFTP.Credentials = new NetworkCredential("FlappyBird", "flappybird");
                FtpWebResponse response = (FtpWebResponse)ReqFTP.GetResponse();
                response.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }

        static public void Download(string filePath, string fileSrc, string fileName, string ftpServerIP, string ftpUserName, string ftpPwd)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            using (FileStream OutputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create))
            {
                FtpWebRequest ReqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + fileSrc));
                ReqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                ReqFTP.UseBinary = true;
                ReqFTP.Credentials = new NetworkCredential(ftpUserName, ftpPwd);
                ReqFTP.KeepAlive = false;
                using (FtpWebResponse response = (FtpWebResponse)ReqFTP.GetResponse())
                {
                    using (Stream FtpStream = response.GetResponseStream())
                    {
                        long Cl = response.ContentLength;
                        int bufferSize = 2048;
                        int readCount;
                        byte[] buffer = new byte[bufferSize];
                        readCount = FtpStream.Read(buffer, 0, bufferSize);
                        while (readCount > 0)
                        {
                            OutputStream.Write(buffer, 0, readCount);
                            readCount = FtpStream.Read(buffer, 0, bufferSize);
                        }
                        FtpStream.Close();
                    }
                    response.Close();
                }
                OutputStream.Close();
            }
        }
        static public void Upload(string sFileDstPath, string FolderName, string ftpServerIP, string ftpUserName, string ftpPwd)
        {
            FileInfo fileInf = new FileInfo(sFileDstPath);
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + FolderName + "/" + fileInf.Name));
            reqFTP.Credentials = new NetworkCredential(ftpUserName, ftpPwd);
            reqFTP.KeepAlive = false;          
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            using (FileStream fs = fileInf.OpenRead())
            {
                using (Stream strm = reqFTP.GetRequestStream())
                {
                    contentLen = fs.Read(buff, 0, buffLength);
                    while (contentLen != 0)
                    {
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                    strm.Close();
                }
                fs.Close();
            }
            reqFTP.Abort();
        }
        static public void Delete(string filePath, string fileName, string ftpServerIP, string ftpUserName, string ftpPwd)
        {
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + filePath + "/" + fileName));
            reqFTP.Credentials = new NetworkCredential(ftpUserName, ftpPwd);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
    }
}
