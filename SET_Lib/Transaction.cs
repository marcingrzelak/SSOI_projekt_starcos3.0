using System;
using System.Collections.Generic;
using System.Text;

namespace Eportmonetka.SET_Lib
{
    class Transaction
    {
        public List<TransacItem> ListOfItems { get; private set; }
        public float Cost { get; private set; }

        private string Date;
        private string UserID;
        private string ShopID;

        public string SignedHash { private get; set; }
        public string Hash { private get; set; }

        public Transaction()
        {
            ListOfItems = new List<TransacItem>();
            Date = DateTime.Now.ToString();
        }
        public Transaction(string _UserID, string _ShopID)
        {
            ListOfItems = new List<TransacItem>();
            Date = DateTime.Now.ToString();
            UserID = _UserID;
            ShopID = _ShopID;
        }
        public Transaction(string _UserID, string _ShopID, string _Time)
        {
            ListOfItems = new List<TransacItem>();
            Date = _Time;
            UserID = _UserID;
            ShopID = _ShopID;
        }
        public void SetCost(float _Cost)
        {
            Cost = _Cost;
        }
        public void AddToTransac(TransacItem NewItem)
        {
            ListOfItems.Add(NewItem);
        }

        public string Print(bool[] RullesArray, short MSGPrintMode)     // Rulles: 1 - print UserId, 2 - print ShopID, 3 - print ItemList, 4 - print Cost
        {                                                              // Modes: 0 - nothing, 1 - MSG 1, 2 - MSG 2
            Cost = 0;
            if (RullesArray.Length != 4)
            {
                return "Error!";
            }
            StringBuilder ToPrint = new StringBuilder();
            ToPrint.Append("<Transaction>\n");
            // Header:
            ToPrint.Append("\t<Info>" + "SET" + "</Info>\n");
            ToPrint.Append("\t<Date>" + Date + "</Date>\n");
            if (RullesArray[0])
            {
                ToPrint.Append("\t<UserID>" + UserID + "</UserID>\n");
            }
            if (RullesArray[1])
            {
                ToPrint.Append("\t<ShopID>" + ShopID + "</ShopID>\n");
            }
            //List of items:
            if (RullesArray[2])
            {
                ToPrint.Append("\t<ItemList>\n");
            }
            foreach (TransacItem x in ListOfItems)
            {
                if (RullesArray[2])
                {
                    ToPrint.Append("\t\t<Product>\n");
                    ToPrint.Append("\t\t\t<Name>" + x.Name + "</Name>\n");
                    ToPrint.Append("\t\t\t<Quantity>" + x.Quantity + "</Quantity>\n");
                    ToPrint.Append("\t\t</Product>\n");
                }
                Cost += x.Quantity * x.Cost;
            }
            if (RullesArray[2])
            {
                ToPrint.Append("\t</ItemList>\n");
            }

            if (RullesArray[3])
            {
                ToPrint.Append("\t<Cost>" + Cost.ToString("0.00") + "</Cost>\n");
            }
            if (MSGPrintMode != 0)
            {
                switch (MSGPrintMode)
                {
                    case 1:
                        ToPrint.Append("\t<H2>" + Hash + "</H2>\n");
                        break;
                    case 2:
                        ToPrint.Append("\t<H1>" + Hash + "</H1>\n");
                        break;
                }
                ToPrint.Append("\t<SignH3>" + SignedHash + "</SignH3>\n");
            }

            ToPrint.Append("</Transaction>");
            return ToPrint.ToString();
        }
    }
}
