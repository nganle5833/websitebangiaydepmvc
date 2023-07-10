using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebBanGiayDep.Models;

namespace WebBanGiayDep.Controllers
{
    public class DonHangController : Controller
    {
        dbShopGiayDataContext data = new dbShopGiayDataContext();

        // GET: DonHang
        public ActionResult LichSuDonHang()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            else
            {
                KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
                var listLichSuDonHang = data.DONHANGs.Where(p => p.MaKH == kh.MaKH).ToList();
                return View(listLichSuDonHang);
            }

        }
        public ActionResult ChiTietDonHang(int? maDonHang, decimal? tongTien)
        {
            if (Session["Taikhoan"] == null) RedirectToAction("Index", "Home");
            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
            var ListChiTietDonHang = data.CT_DONHANGs.Where(p => p.MaDonHang == maDonHang).ToList();  // danh sách các sản phẩm trong chi tiết đơn hàng

            List<CTDH> listCTDH = new List<CTDH>();

            foreach (var item in ListChiTietDonHang)
            {
                listCTDH.Add(new CTDH(item.MaGiay, maDonHang));
            }
            ViewBag.TongTien = tongTien;
            return View(listCTDH);
        }
    }
}