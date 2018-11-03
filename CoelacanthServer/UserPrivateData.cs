using System;
namespace CoelacanthServer
{
    public class UserPrivateData
    {
        private static string _nickname;
        public static string Nickname
        {
            get { return _nickname; }
            set { _nickname = value; }
        }

        private static int _id;
        public static int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private static string _room;
        public static string Room
        {
            get { return _room; }
            set { _room = value; }
        }


    }
}
