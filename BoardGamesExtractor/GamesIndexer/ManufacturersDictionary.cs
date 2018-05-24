using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public class ManufacturersDictionary
    {
        private List<string> Manufacturers;
        private int n;
        public int N { get { return n; } }

        public ManufacturersDictionary()
        {
            Manufacturers = new List<string>();
            n = 0;
        }

        public ManufacturersDictionary(List<string> L)
        {
            Manufacturers = new List<string>();
            n = 0;
            string s;
            int i, M = L.Count;
            for (i = 0; i < N; i++)
            {
                s = L[i].Trim();
                if (s != "")
                    if (!Manufacturers.Contains(s))
                    {
                        n++;
                        Manufacturers.Add(s);
                    }
            }
        }

        public ManufacturersDictionary(string FNameManufacturers, out bool res, out string msg)
        {
            FileIO.ReadStringList(FNameManufacturers, out n, out res, out msg);
            if (!res)
            {
                n = 0;
                Manufacturers = new List<string>();
            }
        }

        public int IndexOf(string Manufacturer)
        {
            return Manufacturers.IndexOf(Manufacturer);
        }

        public void Add(string Manufacturer)
        {
            if (!Manufacturers.Contains(Manufacturer))
            {
                n++;
                Manufacturers.Add(Manufacturer);
            }
        }
    }
}
