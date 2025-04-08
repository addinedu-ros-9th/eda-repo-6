namespace stockdetail
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Data;

public class StockDetail
{
    public string stock_name;
    public string std_code;
    public string abbr;
    public string day;
    public int closing_price;
    public int contrast;
    public float fluctuation_rate;
    public int market_price;
    public int high_price;
    public int low_price;
    public double trading_vol;
    public long trs;
    public long cpt;
    public long num_of_sh;

    public StockDetail(string name, string std, string d, int cl, string abb = "", 
        int co = 0, float fl = 0, int ma = 0, int hi = 0, int lo = 0, double tra = 0, 
        long tr = 0, long cp = 0, long ns = 0)
    {
        stock_name = name;
        std_code = std;
        abbr = abb;
        day = d;
        closing_price = cl;
        contrast = co;
        fluctuation_rate = fl;
        market_price = ma;
        high_price = hi;
        low_price = lo;
        trading_vol = tra;
        trs = tr;
        cpt = cp;
        num_of_sh = ns;
    }
}
}