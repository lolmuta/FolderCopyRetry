using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFolderRetry
{
    class Program
    {
        //模擬
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("讀取db 1筆資料... 取得路徑dest 與 src");
                //複製檔案
                string dest = @"\\192.169.0.1\a";
                string src = @"\\192.169.0.1\b";
                FileUtil.CopyFolderRetry(src, dest);//複製檔案
                Console.WriteLine("更新db 狀態為成功...");
                Console.WriteLine($"刪除{dest}");
            }
            catch (Exception ex)
            {
                //log error
                Console.WriteLine(ex);
                Console.WriteLine("更新db 狀態為失敗...");
                throw;
            }
            //換下一筆 db
        }
    }
}
