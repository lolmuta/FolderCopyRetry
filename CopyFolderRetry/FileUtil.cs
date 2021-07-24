using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopyFolderRetry
{
    class FileUtil
    {
        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        public static void CopyFolderRetry(string src, string dest)
        {
            const int retryMax = 10;//重試次數
            const int delaySec = 3; //重試等待時間(秒)
            long totalFileSizeByte = 0; //total filesize
            int totalFileCount = 0;     //total fileCount
            double copySpeed;       //total copy speend (byte/ms)
            TimeSpan interval = TimeSpan.FromSeconds(delaySec);//計時器，用來計算檔案複製效率
            DirectoryInfo info = new DirectoryInfo(src);

            //確認資料夾是否存在(重試模式)
            Retry.Do(() =>
            {
                if (!info.Exists)
                {
                    throw new Exception($"{src}不存在");
                }
            }, interval, retryMax);

            //計算資料夾檔案數量，檔案總容量(重試模式)            
            var srcDir = info.EnumerateFiles();
            Retry.Do(() =>
            {
                totalFileSizeByte = srcDir.Sum(file => file.Length);
                totalFileCount = srcDir.Count();
            }, interval, retryMax);

            Stopwatch swtch = Stopwatch.StartNew();//計算檔案複製速度
            swtch.Start();  //計時開始
            foreach (var file in srcDir)//資料夾loop 頭
            {
                string destFile = Path.Combine(dest, file.Name);
                int _retryCount = 0;

                do
                {
                    try
                    {
                        file.CopyTo(destFile, true);//複製檔案                        
                        break;  //複製成功，跳出重試迴圈
                    }
                    catch (Exception ex)
                    {
                        swtch.Stop();//失敗停止計時
                        _retryCount++; //錯誤次數+1
                        if (_retryCount > retryMax)
                        {
                            //若重試次數過多則跳出, 並log  
                            throw;
                        }
                        Thread.Sleep(interval);//停頓一下
                        swtch.Start();  //計時開始                       
                    }
                } while (true);
            }//資料夾loop 尾
            swtch.Stop();   //複製完畢，計時中止
            //取得總共花費毫秒
            long totalSpendMs = swtch.ElapsedMilliseconds;
            //計算複製效率 = 總檔案容量 / 花費時間
            copySpeed = (totalFileSizeByte * 1.0) / totalSpendMs;
            Console.WriteLine($"從{src}複製到{dest}");
            Console.WriteLine($"檔案數量{totalFileCount}");
            Console.WriteLine($"檔案總容量{FileUtil.GetBytesReadable(totalFileSizeByte)}");
            Console.WriteLine($"複製速率為{totalFileSizeByte} byte/ms");
        }

    }
}
