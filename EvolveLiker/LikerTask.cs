﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvolveLiker
{
    public class LikerTask
    {
        public string TargetLogin { get; }
        public Dictionary<string, bool> UriList { get; }
        public bool IsComplete => likesCountComplete == LikesCount;
        public int LikesCount { get; }
        private int LikesCountComplete;
        private int likesCountComplete
        {
            get
            {
                return LikesCountComplete;
            }

            set
            {
                LikesCountComplete = value;
                mainWorker.LikerProgress(LikesCountComplete);
            }
        }
        private readonly AccountContainer accountContainer;
        private bool isStop;
        private MainWorker mainWorker;

        public LikerTask(string targetLogin, int likesCount,
            AccountContainer accountContainer, MainWorker mainWorker)
        {
            this.mainWorker = mainWorker;
            LikesCount = likesCount;
            TargetLogin = targetLogin;
            UriList = new Dictionary<string, bool>();
            this.accountContainer = accountContainer;
            isStop = false;
        }

        public void AddUri(string uri)
        {
            UriList.Add(uri, false);
        }

        public void Start()
        {
            isStop = false;
            while (!isStop && !IsComplete && UriList.Where((k, v) => !k.Value).Count() != 0)
            {
                var uri = UriList.Where((k, v) => !k.Value).FirstOrDefault().Key;
                UriList[uri] = true;
                var likesComplete = accountContainer.PutLikes(uri, TargetLogin, LikesCount - likesCountComplete);
                likesCountComplete += likesComplete;
                SleepWithStopFlag(1000 * 60 * 60 * 2);
            }
        }

        public void Stop()
        {
            isStop = true;
        }

        private void SleepWithStopFlag(int ms)
        {
            var delay = Math.Min(ms, 1000);
            var delayComplete = 0;
            while (delayComplete < ms)
            {
                Thread.Sleep(delay);
                delayComplete += delay;

                if (isStop)
                    break;
            }
        }
    }
}