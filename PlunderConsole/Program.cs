﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plunder;
using Plunder.Compoment;
using Plunder.Scheduler;
using Plunder.Downloader;
using Plunder.Plugin.Analyze;
using Plunder.Plugin.Compoment;
using Plunder.Plugin.Pipeline;
using Plunder.Plugin.Downloader;

namespace PlunderConsole
{
    public class Program
    {
        static Spider _spider;
        static void Main(string[] args)
        {
            RunSpider();
        }
        static void RunSpider()
        {
            _spider = new Spider(new SequenceScheduler());
            var downloaders = new List<IDownloader> { new HttpClientDownloader(4) };
            _spider.RegisterDownloader(downloaders);
            _spider.RegisterPageAnalyzer<UsashopcnPageAnalyzer>(UsashopcnPageAnalyzer.SiteId);
            _spider.RegisterPipeModule(new ConsoleModule(500, 0, 400, 500, true, true));


            _spider.Start(TopicType.StaticHtml, SiteIndex.Usashopcn, "http://www.usashopcn.com/");

            var statusTimer = new Timer(spider => { Console.WriteLine(((Spider) spider).RunStatusInfo()); }, _spider, 0, 2000);
        }


        #region 控制台多缓冲区测试

        static void ConsoleMultiAreaTest()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine(i + "AAAAA AAAAA AAAAA AAAAA" + i);
            }


            for (int i = 0; i < 100; i++)
            {
                if (i > 19)
                    Console.SetCursorPosition(50, 19);
                else
                    Console.SetCursorPosition(50, i);

                Thread.Sleep(300);
                Console.WriteLine(i + "BBBBB BBBBB BBBBB BBBBB" + i);
                if (i > 19)
                {
                    Console.SetCursorPosition(50, 19);
                    Console.MoveBufferArea(50, 1, 25, 20, 50, 0);
                }

            }

            Console.ReadKey();
        }

        #endregion

    }
}   