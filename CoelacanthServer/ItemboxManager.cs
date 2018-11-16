using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoelacanthServer
{
    public class ItemboxManager
    {
        ItemboxInitialtize initialize;
        public ItemboxManager()
        {
            initialize = new ItemboxInitialtize();
        }
    }

    public class ItemboxInitialtize
    {
        public ItemboxInitialtize()
        {
            RegenTime = 5.0f;
            RegenTimerEnable = true;
            StartBoxCount = 6;
        }

        private float _regenTime;
        public float RegenTime
        {
            get { return _regenTime; }
            set { _regenTime = value; }
        }

        private bool _regenTimerEnable;
        public bool RegenTimerEnable
        {
            get { return _regenTimerEnable; }
            set { _regenTimerEnable = value; }
        }

        private int _startBoxCount;
        public int StartBoxCount
        {
            get { return _startBoxCount; }
            set { _startBoxCount = value; }
        }

        private float _itemboxPosX;
        public float ItemboxPosX
        {
            get { return _itemboxPosX; }
            set { _itemboxPosX = value; }
        }

        private float _itemboxPosZ;
        public float ItemboxPosZ
        {
            get { return _itemboxPosZ; }
            set { _itemboxPosZ = value; }
        }
    }
}
