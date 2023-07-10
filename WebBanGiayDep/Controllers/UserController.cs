using System;
using System.Linq;
using System.Web.Mvc;
using WebBanGiayDep.Models;

namespace WebBanGiayDep.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        dbShopGiayDataContext data = new dbShopGiayDataContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Dangky()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Dangky(FormCollection collection, KHACHHANG kh)
        {
            var hoten = collection["HotenKH"];
            var tendn = collection["TenDN"];
            var matkhau = collection["MatKhau"];
            var matkhaunhaplai = collection["MatkhauNhapLai"];
            var email = collection["Email"];
            var diachi = collection["DiaChi"];
            var dienthoai = collection["DienThoai"];
            var ngaysinh = String.Format("{0:MM/dd/yyyy}", collection["NgaySinh"]);
            string matkhau_mahoa;


            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Họ Tên khách hàng không được để trống";
            }
            else if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi2"] = "Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi3"] = "Mật khẩu không được để trống!";
            }
            else if (!matkhaunhaplai.Equals(matkhau))
            {
                ViewData["Loi4"] = "Mật khẩu nhập lại không trùng khớp!";
            }
            else if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi5"] = "Email không được để trống";
            }
            else if (String.IsNullOrEmpty(diachi))
            {
                ViewData["Loi6"] = "Địa chỉ không được để trống";
            }
            else if (String.IsNullOrEmpty(dienthoai))
            {
                ViewData["Loi7"] = "Số điện thoại không được để trống";
            }
            else if (String.IsNullOrEmpty(ngaysinh))
            {
                ViewData["Loi8"] = "Vui lòng nhập ngày sinh!!!";
            }
            else
            {
                matkhau_mahoa = Md5.MaHoaMD5(matkhau);
                kh.HoTen = hoten;
                kh.TaiKhoanKH = tendn;
                kh.MatKhau = matkhau_mahoa;
                kh.EmailKH = email;
                kh.DiaChiKH = diachi;
                kh.NgaySinh = DateTime.Parse(ngaysinh);
                kh.DienThoaiKH = dienthoai;
                kh.TrangThai = true;

                data.KHACHHANGs.InsertOnSubmit(kh);
                data.SubmitChanges();
                return RedirectToAction("Dangnhap", "User");
            }

            return this.Dangky();
        }
        [HttpGet]
        public ActionResult Dangnhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Dangnhap(FormCollection collection)
        {
            var tendn = collection["TenDN"];
            var matkhau = collection["MatKhau"];
            var matkhau_mahoa = Md5.MaHoaMD5(matkhau);
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được để trống";
            }
            else
            {
                KHACHHANG kh = data.KHACHHANGs.SingleOrDefault(n => n.TaiKhoanKH == tendn && n.MatKhau == matkhau_mahoa);
                if (kh != null)
                {
                    if (kh.TrangThai == true)
                    {
                        ViewBag.Thongbao = "Chúc mừng đăng nhập thành công";
                        Session["TaiKhoan"] = kh;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Thongbao = "Tài khoản đã bị khóa";
                    }
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
        //y kien khach hang
        [HttpGet]
        public ActionResult Themmoiykien()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Themmoiykien(YKIENKHACHHANG yKIENKHACHHANG)
        {
            data.YKIENKHACHHANGs.InsertOnSubmit(yKIENKHACHHANG);
            data.SubmitChanges();
            return RedirectToAction("Index", "Home");
        }
        //doi mat khau
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DoiMatKhau(FormCollection collection)
        {
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
            {
                return RedirectToAction("Dangnhap", "User");
            }
            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];

            var user = data.KHACHHANGs.SingleOrDefault(p => p.MaKH == kh.MaKH);

            var oldPassword = collection["oldPassword"];
            var newPassword = collection["newPassword"];
            var confirmNewPassword = collection["confirmNewPassword"];

            if (String.IsNullOrEmpty(oldPassword) || String.IsNullOrEmpty(newPassword) || // trống textbox
                String.IsNullOrEmpty(confirmNewPassword))
            {
                ViewData["Error"] = "Vui lòng điền đủ thông tin";
                return this.DoiMatKhau();
            }
            else if (!String.Equals(newPassword, confirmNewPassword)) // 2 ô mật khẩu mới không khớp
            {
                ViewData["Error"] = "Mật khẩu mới không khớp";
                return this.DoiMatKhau();
            }
            else if (!String.Equals(Md5.MaHoaMD5(oldPassword), user.MatKhau)) // kiểm tra mật khẩu cũ
            {
                ViewData["Error"] = "Sai mật khẩu cũ";
                return this.DoiMatKhau();
            }
            else // ==============thay đổi mật khẩu===================
            {
                newPassword = Md5.MaHoaMD5(newPassword);
                user.MatKhau = newPassword;
                data.SubmitChanges();
                Session["Taikhoan"] = user;
                ViewData["Success"] = "Đổi mật khẩu thành công";
                return this.DoiMatKhau();
            }
        }
        //dang xuat
        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        //chinh sua thong tin khách hàng
        [HttpGet]
        public ActionResult MyProfile(int id) // chuyển đến trang hồ sơ
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            else
            {
                KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
                ViewBag.MaKH = kh.MaKH;
                return View(kh);
            }

        }
        [HttpPost, ActionName("MyProfile")]
        public ActionResult UpdateProfile(int id)   
        {
            KHACHHANG kh = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            UpdateModel(kh);
            data.SubmitChanges();
            Session["Taikhoan"] = kh;
            return Content("<script>alert('Thay đổi thông tin thành công');window.location='/User/MyProfile/" + kh.MaKH + "';</script>");
        }
    }
}