using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebBanGiayDep.Models;

namespace WebBanGiayDep.Controllers
{
    public class HomeController : Controller
    {
        //Tao 1 doi tuong chua toan bo CSDL tu dbWeb ban giay
        dbShopGiayDataContext data = new dbShopGiayDataContext();
        #region Trang chủ
        #region index
        public ActionResult Index(int? page)
        {
            int pageSize = 9;
            int pageNum = (page ?? 1);
            var giayton = from s in data.SANPHAMs
                          where s.TrangThai == true
                          select s;
            return PartialView(giayton.ToPagedList(pageNum, pageSize));
        }
        #endregion
        #region danh sách sản phẩm
        public ActionResult ListSanPham(int? page)
        {
            int pageSize = 9;
            int pageNum = (page ?? 1);
            var giayton = from s in data.SANPHAMs join lg in data.LOAIGIAYs on s.MaLoai equals lg.MaLoai 
                          where s.TrangThai == true && lg.TrangThai == true
                          select s;
            return PartialView(giayton.ToPagedList(pageNum, pageSize));
        }
        #endregion
        #region lấy giày mới
        private List<SANPHAM> LayGiayMoi(int count)
        {
            return data.SANPHAMs.OrderByDescending(a => a.NgayCapNhat).Take(count).ToList();
        }
        #endregion
        #region giày mới
        public ActionResult GiayMoi()
        {
            var giaymoi = LayGiayMoi(5);
            return PartialView(giaymoi);
        }
        #endregion
        #region số lượng giày tồn
        private List<SANPHAM> SoLuongTonGiay(int count)
        {
            return data.SANPHAMs.OrderByDescending(a => a.SoLuongTon).Take(count).ToList();
        }
        #endregion
        #region chi tiết
        public ActionResult ChiTietSanPham(int id)
        {
            var chitietsanpham = (from s in data.SANPHAMs
                                  where s.MaGiay == id
                                  join lg in data.LOAIGIAYs
                                  on s.MaLoai equals lg.MaLoai
                                  join th in data.THUONGHIEUs
                                  on s.MaThuongHieu equals th.MaThuongHieu
                                  select new CTSP
                                  {
                                      MaGiay = s.MaGiay,
                                      TenGiay = s.TenGiay,
                                      Size = s.Size,
                                      AnhBia = s.AnhBia,
                                      GiaBan = s.GiaBan,
                                      MaThuongHieu = th.MaThuongHieu,
                                      TenThuongHieu = th.TenThuongHieu,
                                      MaLoai = lg.MaLoai,
                                      TenLoai = lg.TenLoai,
                                      ThoiGianBaoHanh = (int)s.ThoiGianBaoHanh,
                                  });
            return View(chitietsanpham.Single());
        }

        #endregion
        #region thương hiệu
        public ActionResult ThuongHieu()
        {
            var thuonghieu = (from s in data.THUONGHIEUs select s);
            return PartialView(thuonghieu);
        }
        #endregion
        #region sản phẩm theo thương hiệu
        public ActionResult SPTheoThuongHieu(int id, int? page)
        {
            int pageSize = 9;
            int pageNum = (page ?? 1);
            var sanpham = from s in data.SANPHAMs where (s.MaThuongHieu == id && s.TrangThai == true) select s;
            return View(sanpham.ToPagedList(pageNum, pageSize));
        }
        #endregion
        #region giày nam
        public ActionResult GiayNam()
        {
            var GiayNam = from s in data.LOAIGIAYs 
                          where s.GioiTinh == true && s.TrangThai == true
                          select s;
            return PartialView(GiayNam);
        }
        #endregion
        #region giày nữ

        public ActionResult GiayNu()
        {
            var GiayNu = from s in data.LOAIGIAYs
                         where s.GioiTinh == false && s.TrangThai == true
                         select s;
            return PartialView(GiayNu);
        }
        #endregion
        #region giới tính
        public ActionResult SPTheoGioiTinh(int id, int? page)
        {
            int pageSize = 9;
            int pageNum = (page ?? 1);
            var sanpham = from s in data.SANPHAMs where (s.MaLoai == id && s.TrangThai == true) select s;
            return View(sanpham.ToPagedList(pageNum, pageSize));
        }
        #endregion
        #region tin tức
        public ActionResult TinTuc()
        {
            return View();
        }
        #endregion
        #region giới thiệu
        public ActionResult GioiThieu()
        {
            return View();
        }
        #endregion
        #region Sản phẩm tìm kiếm (Search)
        public ActionResult Search(string id)
        {
            //Lấy ra danh sách sản phẩm từ chuỗi tìm kiếm truyền vào
            var search = (from sp in data.SANPHAMs
                          where (sp.TrangThai == true) && (sp.TenGiay.Contains(id)) && (sp.THUONGHIEU.TenThuongHieu.Contains(id))
                          orderby sp.GiaBan descending
                          select sp).ToList();

            //Lấy ra các sản phẩm khác (Không có sản phẩm của nhà sản xuất đang xem)
            ViewBag.SP_Khac = (from sp in data.SANPHAMs
                               where (sp.TrangThai == true) && (!sp.TenGiay.Contains(id)) && (sp.THUONGHIEU.TenThuongHieu.Contains(id))
                               orderby sp.GiaBan descending
                               select sp).ToList();

            //Tạo key để xuất thông báo tìm kiếm
            ViewBag.TuKhoa = id;
            return View(search);
        }
        #endregion
        #endregion
    }
}