using System;
using System.Linq;

namespace WebBanGiayDep.Models
{
    public class CTDH
    {
        public int iMaGiay { get; set; }
        public string iTenGIay { get; set; }
        public string iAnhBia { get; set; }
        public Decimal iGiaLucBan { get; set; }
        public int iSoLuong { set; get; }
        public int iSize { get; set; }

        public Decimal iThanhTien { get; set; }

        public CTDH(int maGiay, int? orderID)
        {
            dbShopGiayDataContext data = new dbShopGiayDataContext();
            iMaGiay = maGiay;
            iOrderID = orderID;
            SANPHAM sp = data.SANPHAMs.Single(n => n.MaGiay == maGiay);
            iTenGIay = sp.TenGiay;
            iAnhBia = sp.AnhBia;
            iSize = sp.Size;
            var ctdh = data.CT_DONHANGs.Single(p => p.MaGiay == maGiay && p.MaDonHang == iOrderID); // gán giá bán bên chi tiết đơn hàng => khi thay đổi giá sản phẩm thì vẫn lưu giá cũ của đơn hàng cũ
            iGiaLucBan = (decimal)ctdh.GiaLucBan;
            iThanhTien = (decimal)ctdh.ThanhTien;
            iSoLuong = ctdh.SoLuong;

            var order = data.DONHANGs.SingleOrDefault(p => p.MaDonHang == orderID);
            if (order != null) iUserID = order.MaKH;
        }
        //  ============  các giá trị bên controler admin ==============
        public int? iUserID { get; set; } // action orderdetail
        public int? iOrderID { get; set; }
    }
}