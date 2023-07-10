using PagedList;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanGiayDep.Models;

namespace WebBanGiayDep.Controllers
{

    public class AdminController : Controller
    {
        dbShopGiayDataContext data = new dbShopGiayDataContext();
        #region Trang chủ
        public ActionResult Index()
        {
            if (Session["Username_Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            var kh = data.KHACHHANGs.ToList();
            var ct = data.CT_DONHANGs.ToList();
            var dh = data.DONHANGs.ToList();
            decimal sum = 0;
            foreach (var item in dh)
            {
                if (item.TinhTrangGiaoHang == true)
                {
                    sum = (decimal)dh.Sum(n => n.TongTien);
                }
            }
            var delivery = (from s in data.DONHANGs
                            where s.TinhTrangGiaoHang == true
                            select s).ToList();

            ViewBag.TotalDeli = delivery.Count;
            ViewBag.TotalEarnings = sum;
            ViewBag.TotalUser = kh.Count;
            ViewBag.Total = ct.Count;
            return View();
        }
        #endregion
        #region Đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Username_Admin"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            try
            {
                string Username = collection["txt_Username"];
                string Password = collection["txt_Password"];
                var AdminLogin = data.QUANLies.SingleOrDefault(a => a.TaiKhoanQL == Username && a.MatKhau == Password);
                if (ModelState.IsValid && AdminLogin != null)
                {
                    if (AdminLogin.TrangThai == true)// tài khoản không bị ban
                    {
                        //Lưu các thông tin vào Session
                        Session.Add("MaAdmin", AdminLogin.MaQL);
                        Session.Add("Username_Admin", Username);
                        Session.Add("HoTen_Admin", AdminLogin.HoTen);
                        Session.Add("Avatar_Admin", AdminLogin.Avatar);
                        //Lấy ra thông tin phân quyền của tài khoản vừa Login và vào Session
                        var PhanQuyen = data.PHANQUYENs.SingleOrDefault(p => p.MaQL == int.Parse(Session["MaAdmin"].ToString()));
                        Session.Add("PQ_QuanTriAdmin", PhanQuyen.QL_Admin);
                        Session.Add("PQ_KhachHang", PhanQuyen.QL_KhachHang);
                        Session.Add("PQ_YKienKhachHang", PhanQuyen.QL_YKienKhachHang);
                        Session.Add("PQ_DonHang", PhanQuyen.QL_DonHang);
                        Session.Add("PQ_ThuongHieu", PhanQuyen.QL_ThuongHieu);
                        Session.Add("PQ_NhaCungCap", PhanQuyen.QL_NhaCungCap);
                        Session.Add("PQ_LoaiGiay", PhanQuyen.QL_LoaiGiay);
                        Session.Add("PQ_SanPham", PhanQuyen.QL_SanPham);

                        return RedirectToAction("Index", "Admin");
                    }
                    else { return Content("<script>alert('Tài khoản quản trị của bạn đã bị khóa!');window.location='/Admin/Login';</script>"); }
                }
                else { return Content("<script>alert('Tên đăng nhập hoặc mật khẩu không đúng!');window.location='/Admin/Login';</script>"); }
            }
            catch
            {
                return Content("<script>alert('Đăng nhập thất bại!');window.location='/Admin/Login';</script>");
            }
        }
        #endregion
        #region Đăng xuất
        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login");
        }
        #endregion
        #region Thông tin tài khoản
        public ActionResult Account()
        {
            //Chưa đăng nhập => Login
            if (Session["Username_Admin"] == null)
            {
                return RedirectToAction("Login");
            }

      

            string username = Session["Username_Admin"].ToString();
            ViewBag.Username = username;

            int MaAdmin = int.Parse(Session["MaAdmin"].ToString());
            var ttad = data.QUANLies.SingleOrDefault(a => a.MaQL == MaAdmin);
            return View(ttad);
        }
        [HttpPost]
        public ActionResult Account(FormCollection collection)
        {
            try
            {
                string Email = collection["txt_Email"];
                string HoTen = collection["txt_HoTen"];
                string DienThoai = collection["txt_DienThoai"];
                string TaiKhoan = collection["Username_Admin"];

                int MaAdmin = int.Parse(Session["MaAdmin"].ToString());
                var ttad = data.QUANLies.SingleOrDefault(a => a.MaQL == MaAdmin);
                //Gán giá trị để chỉnh sửa
                ttad.EmailQL = Email;
                ttad.HoTen = HoTen;
                ttad.DienThoaiQL = DienThoai;
                HttpPostedFileBase FileUpload = Request.Files["FileUpload"];
                if (FileUpload != null && FileUpload.ContentLength > 0)//Kiểm tra đã chọn 1 file Upload để thực hiện tiếp
                {
                    string FileName = Path.GetFileName(FileUpload.FileName);
                    string Link = Path.Combine(Server.MapPath("/images/Upload/"), FileName);
                    if (FileUpload.ContentLength > 1 * 1024 * 1024)
                    {
                        return Content("<script>alert('Kích thước của tập tin không được vượt quá 1 MB!');window.location='/Admin/Account';</script>");
                    }
                    var DuoiFile = new[] { "jpg", "jpeg", "png", "gif" };
                    var FileExt = Path.GetExtension(FileUpload.FileName).Substring(1);
                    if (!DuoiFile.Contains(FileExt))
                    {
                        return Content("<script>alert('Chỉ được tải tập tin hình ảnh dạng (.jpg, .jpeg, .png, .gif)!');window.location='/Admin/Account';</script>");
                    }
                    FileUpload.SaveAs(Link);
                    ttad.Avatar = "/images/Upload/" + FileName;
                }
                //Thực hiện chỉnh sửa
                UpdateModel(ttad);
                data.SubmitChanges();
                return Content("<script>alert('Cập nhật thông tin cá nhân thành công!');window.location='/Admin/Account';</script>");
            }
            catch
            {
                return Content("<script>alert('Lỗi hệ thống.Vui lòng thử lại!');window.location='/Admin/Account';</script>");
            }
        }
        #endregion
        #region Đổi mật khẩu (ChangePassword)
        public ActionResult ChangePassword()
        {
            if (Session["Username_Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(FormCollection collection)
        {
            try
            {
                //Lấy giá trị ở Form ChangePassword
                string _PassOld = collection["txt_Password"];
                string _PassNew = collection["txt_PasswordNew"];
                string _RePassNew = collection["txt_NhapLaiPass"];
                int _MaAdmin = int.Parse(Session["MaAdmin"].ToString());
                var ad = data.QUANLies.SingleOrDefault(a => a.MaQL == _MaAdmin);
                if (ad.MatKhau == _PassOld)
                {
                    if (_RePassNew == _PassNew)
                    {
                        if (_PassNew.Length >= 6)
                        {
                            ad.MatKhau = _PassNew;
                            UpdateModel(ad);
                            data.SubmitChanges();
                            return Content("<script>alert('Đổi mật khẩu thành công!');window.location='/Admin/ChangePassword';</script>");
                        }
                        else
                            return Content("<script>alert('Mật khẩu mới phải có ít nhất 6 ký tự!');window.location='/Admin/ChangePassword';</script>");
                    }
                    else
                        return Content("<script>alert('Mật khẩu nhập lại không đúng!');window.location='/Admin/ChangePassword';</script>");
                }
                else
                    return Content("<script>alert('Mật Khẩu cũ không đúng!');window.location='/Admin/ChangePassword';</script>");
            }
            catch
            {
                return Content("<script>alert('Thao tác đổi mật khẩu thất bại!');window.location='/Admin/ChangePassword';</script>");
            }
        }
        #endregion
        #region Quản trị Admin(Tạo thêm mới + ẩn trạng thái + phân quyền)
        #region Danh sách admin
        public ActionResult ListAdmin(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
                if (bool.Parse(Session["PQ_QuanTriAdmin"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực quản trị Administrator !');window.location='/Admin/';</script>");
            }

            int PageSize = 4;//Chỉ lấy ra 3 dòng (3 Admin)
            int PageNum = (page ?? 1);

            //Lấy ra Danh sách Admin
            var PQ = (from pq in data.PHANQUYENs
                      where pq.MaQL > 1
                      orderby pq.MaQL ascending
                      select pq).ToPagedList(PageNum, PageSize);
            return View(PQ);
        }
        #endregion
        #region Quản lý trạng thái Admin
        [HttpPost]
        public void UpdateTrangThai(int id)
        {
            var _AD = (from ad in data.QUANLies where ad.MaQL == id select ad).SingleOrDefault();
            string _Hinh = "";
            if (_AD.TrangThai == true)
            {
                _AD.TrangThai = false;
                _Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _AD.TrangThai = true;
                _Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_AD);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        #region Quản trị Admin
        [HttpPost]
        public void UpdatePQ_QuanTriAdmin(int id)
        {
            //Lấy ra tài khoản Admin trong bảng phân quyền cần Update quyền quản trị
            var _PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            //Tạo chuỗi _Hinh để chứa đường dẫn hình Ẩn Hiện khi Update lại
            string _Hinh = "";

            //Không có quyền thì cập nhật lại thành có và ngược lại
            if (_PQ.QL_Admin == true)
            {
                _PQ.QL_Admin = false;
                _Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                _PQ.QL_Admin = true;
                _Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(_PQ);
            data.SubmitChanges();
            Response.Write(_Hinh);

        }
        #endregion
        #region Quản lý nhà cung cấp
        [HttpPost]
        public void UpdatePQ_NhaSanXuat(int id)
        {
            var _PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string _Hinh = "";
            if (_PQ.QL_NhaCungCap == true)
            {
                _PQ.QL_NhaCungCap = false;
                _Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                _PQ.QL_NhaCungCap = true;
                _Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(_PQ);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        #region quản lý sản phẩm
        [HttpPost]
        public void UpdatePQ_SanPham(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_SanPham == true)
            {
                PQ.QL_SanPham = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_SanPham = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Quản lý khách hàng
        [HttpPost]
        public void UpdatePQ_KhachHang(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_KhachHang == true)
            {
                PQ.QL_KhachHang = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_KhachHang = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Quản lý đơn hàng
        [HttpPost]
        public void UpdatePQ_DonHang(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_DonHang == true)
            {
                PQ.QL_DonHang = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_DonHang = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Quản lý thương hiệu
        [HttpPost]
        public void UpdatePQ_ThuongHieu(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_ThuongHieu == true)
            {
                PQ.QL_ThuongHieu = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_ThuongHieu = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Quản lý loại giày
        //Hàm Update Phân quyền cho quản trị:(ở đây sử dụng hàm void để Response.Write hình update lại)
        [HttpPost]
        public void UpdatePQ_LoaiGiay(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_LoaiGiay == true)
            {
                PQ.QL_LoaiGiay = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_LoaiGiay = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Quản lý ý kiến khách hàng
        //Hàm Update Phân quyền cho quản trị:(ở đây sử dụng hàm void để Response.Write hình update lại)
        [HttpPost]
        public void UpdatePQ_YKienKhachHang(int id)
        {
            var PQ = (from ad in data.PHANQUYENs where ad.MaQL == id select ad).SingleOrDefault();
            string Hinh = "";
            if (PQ.QL_YKienKhachHang == true)
            {
                PQ.QL_YKienKhachHang = false;
                Hinh = "/images/Admin/Icons/block.png";
            }
            else
            {
                PQ.QL_YKienKhachHang = true;
                Hinh = "/images/Admin/Icons/accept.png";
            }
            UpdateModel(PQ);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region CreateAdmin
        public ActionResult CreateAdmin()
        {
            if (Session["Username_Admin"] == null)//Chưa đăng nhập => Login
            {
                return RedirectToAction("Login");
            }
            else
            {
                if (bool.Parse(Session["PQ_QuanTriAdmin"].ToString()) == false)//Không đủ quyền hạn vào ku vực này  
                {
                    return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực quản trị Administrator !');window.location='/Admin/';</script>");
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult CreateAdmin(FormCollection collection, QUANLY ad, PHANQUYEN pq)
        {
            try
            {
                //Lấy giá trị ở Form Register         
                string Username = collection["txt_Username"];
                string Password = collection["txt_Password"];
                string RePassword = collection["txt_NhapLaiPass"];
                string Email = collection["txt_Email"];
                string HoTen = collection["txt_HoTen"];
                string DienThoai = collection["txt_DienThoai"];
                string matkhau_mahoa;
                //Kiểm tra xem tài khoản đã có người sử dụng chưa?
                var CheckUser = data.QUANLies.FirstOrDefault(a => a.TaiKhoanQL == Username);
                if (CheckUser != null)
                {
                    return Content("<script>alert('Tên đăng nhập đã có người sử dụng!');window.location='/Admin/CreateAdmin';</script>");
                }
                else
                {
                    ad.TaiKhoanQL = Username;
                }
                if (RePassword != Password)
                    return Content("<script>alert('Mật khẩu nhập lại không đúng!');window.location='/Admin/CreateAdmin';</script>");
                else
                {
                    matkhau_mahoa = Md5.MaHoaMD5(Password);
                    ad.MatKhau = matkhau_mahoa;
                }
                var CheckEmail = data.QUANLies.FirstOrDefault(a => a.EmailQL == Email);
                if (CheckEmail != null)
                {
                    return Content("<script>alert('Email đã có người sử dụng!');window.location='/Admin/CreateAdmin';</script>");
                }
                else
                {
                    ad.EmailQL = Email;
                }
                ad.HoTen = HoTen;
                ad.DienThoaiQL = DienThoai;

                HttpPostedFileBase FileUpload = Request.Files["FileUpload"];
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(FileUpload.FileName);
                    string _Path = Path.Combine(Server.MapPath("/images/Upload/"), _FileName);
                    if (FileUpload.ContentLength > 1 * 1024 * 1024)
                    {
                        return Content("<script>alert('Kích thước của tập tin không được vượt quá 1 MB!');window.location='/Admin/CreateAdmin';</script>");
                    }
                    var DuoiFile = new[] { "jpg", "jpeg", "png", "gif" };
                    var FileExt = Path.GetExtension(FileUpload.FileName).Substring(1);
                    if (!DuoiFile.Contains(FileExt))
                    {
                        return Content("<script>alert('Bảo mật Website! Chỉ được Upload tập tin hình ảnh dạng (.jpg, .jpeg, .png, .gif)!');window.location='/Admin/CreateAdmin';</script>");
                    }
                    FileUpload.SaveAs(_Path);
                    ad.Avatar = "/images/Upload/" + _FileName;
                }
                else
                {
                    ad.Avatar = "/images/Upload/avatars.jpg";
                }
                ad.TrangThai = true;
                data.QUANLies.InsertOnSubmit(ad);
                data.SubmitChanges();
                pq.MaQL = ad.MaQL;
                if (collection["ckb_PhanQuyen1"] == "DaChon")
                {
                    pq.QL_Admin = true;
                }
                else
                {
                    pq.QL_Admin = false;
                }
                if (collection["ckb_PhanQuyen2"] == "DaChon")
                {
                    pq.QL_NhaCungCap = true;
                }
                else
                {
                    pq.QL_NhaCungCap = false;
                }

                if (collection["ckb_PhanQuyen3"] == "DaChon")
                {
                    pq.QL_SanPham = true;
                }
                else
                {
                    pq.QL_SanPham = false;
                }

                if (collection["ckb_PhanQuyen4"] == "DaChon")
                {
                    pq.QL_ThuongHieu = true;
                }
                else
                {
                    pq.QL_ThuongHieu = false;
                }

                if (collection["ckb_PhanQuyen5"] == "DaChon")
                {
                    pq.QL_LoaiGiay = true;
                }
                else
                {
                    pq.QL_LoaiGiay = false;
                }

                if (collection["ckb_PhanQuyen6"] == "DaChon")
                {
                    pq.QL_DonHang = true;
                }
                else
                {
                    pq.QL_DonHang = false;
                }

                if (collection["ckb_PhanQuyen7"] == "DaChon")
                {
                    pq.QL_KhachHang = true;
                }
                else
                {
                    pq.QL_KhachHang = false;
                }

                if (collection["ckb_PhanQuyen8"] == "DaChon")
                {
                    pq.QL_YKienKhachHang = true;
                }
                else
                {
                    pq.QL_YKienKhachHang = false;
                }
                data.PHANQUYENs.InsertOnSubmit(pq);
                data.SubmitChanges();
                return Content("<script>alert('Thêm mới tài khoản quản trị thành công !');window.location='/Admin/ListAdmin';</script>");
            }
            catch
            {
                return Content("<script>alert('Đăng ký thất bại.Hệ thống gặp vấn đề');window.location='/Admin/CreateAdmin';</script>");
            }
        }
        #endregion
        #endregion
        #region Quản lý sản phẩm
        // =================================================Sản Phẩm===================================================
        public ActionResult SanPham(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_SanPham"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực quản lý sản phẩm !');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 7;
            return View(data.SANPHAMs.ToList().OrderBy(n => n.MaGiay).ToPagedList(pageNumber, pageSize));
        }
        #region Trạng thái sản phẩm
        [HttpPost]
        public void TrangThaiSanPham(int id)
        {
            var _DH = (from d in data.SANPHAMs where d.MaGiay == id select d).SingleOrDefault();
            string _Hinh = "";
            if (_DH.TrangThai == true)
            {
                _DH.TrangThai = false;
                _Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _DH.TrangThai = true;
                _Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_DH);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        [HttpGet]
        public ActionResult ThemMoiSanPham()
        {
            //lay ds tu table THUONGHIEU, sap xep theo Ten thuong hieu, chon lay gia tri MaThuongHieu, hien thi ten thuong hieu
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");

            //lay ds tu table LoaiGiay, sap xep theo TenLoaiGiay, chon lay gia tri MaLoai, hien thi TenLoai
            ViewBag.MaLoai = new SelectList(data.LOAIGIAYs.Where(n => n.TrangThai == true).ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");

            //lay ds tu table NHACUNGCAP, sap xep theo TenNCC, chon lay gia tri MaNCC, hien thi TenNCC
            ViewBag.MaNCC = new SelectList(data.NHACUNGCAPs.Where(n => n.TrangThai == true).ToList().OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoiSanPham(SANPHAM sanpham, HttpPostedFileBase fileUpload)
        {
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            ViewBag.MaLoai = new SelectList(data.LOAIGIAYs.ToList().Where(n => n.TrangThai == true).OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNCC = new SelectList(data.NHACUNGCAPs.ToList().Where(n => n.TrangThai == true).OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");

            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //Luu ten file
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    //Luu duong dan cua file
                    var path = Path.Combine(Server.MapPath("~/images"), fileName);
                    //Kiem tra hinh anh co ton tai chua
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        //Luu hinh anh vao duong dan
                        fileUpload.SaveAs(path);
                    }
                    sanpham.AnhBia = fileName;
                    //luu vao csdl
                    data.SANPHAMs.InsertOnSubmit(sanpham);
                    data.SubmitChanges();
                }
                return RedirectToAction("SanPham");
            }
        }

        //Hien Thi Chi Tiet San Pham
        public ActionResult ChiTietSanPham(int id)
        {
            // lay sp theo ma sp
            SANPHAM sanPham = data.SANPHAMs.SingleOrDefault(n => n.MaGiay == id);
            ViewBag.MaGiay = sanPham.MaGiay;
            if (sanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sanPham);
        }

        [HttpGet]
        public ActionResult XoaSanPham(int id)
        {
            //lay san pham can xoa
            SANPHAM sanPham = data.SANPHAMs.SingleOrDefault(n => n.MaGiay == id);
            ViewBag.MaGiay = sanPham.MaGiay;
            if (sanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sanPham);
        }
        [HttpPost, ActionName("XoaSanPham")]
        public ActionResult XacNhanXoa(int id)
        {
            SANPHAM sanPham = data.SANPHAMs.SingleOrDefault(n => n.MaGiay == id);
            ViewBag.MaGiay = sanPham.MaGiay;
            if (sanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.SANPHAMs.DeleteOnSubmit(sanPham);
            data.SubmitChanges();
            return RedirectToAction("SanPham");
        }

        //Chinh sua San pham
        [HttpGet]
        public ActionResult SuaSanPham(int id)
        {
            SANPHAM sanPham = data.SANPHAMs.SingleOrDefault(n => n.MaGiay == id);
            if (sanPham == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //dua du lieu vao drop downlist TenTHuongHieu, TenLoai, TenNCC
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            ViewBag.MaLoai = new SelectList(data.LOAIGIAYs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNCC = new SelectList(data.NHACUNGCAPs.ToList().OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");
            return View(sanPham);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaSanPham(int id, HttpPostedFileBase fileUpload)
        {
            SANPHAM sp = data.SANPHAMs.SingleOrDefault(n => n.MaGiay == id);

            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            ViewBag.MaLoai = new SelectList(data.LOAIGIAYs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNCC = new SelectList(data.NHACUNGCAPs.ToList().OrderBy(n => n.TenNCC), "MaNCC", "TenNCC");
            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //Luu ten file
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    //Luu duong dan cua file
                    var path = Path.Combine(Server.MapPath("~/images"), fileName);
                    //Kiem tra hinh anh co ton tai chua
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        //Luu hinh anh vao duong dan
                        fileUpload.SaveAs(path);
                    }
                    sp.AnhBia = fileName;
                    //luu vao csdl
                    UpdateModel(sp);
                    data.SubmitChanges();
                }
                return RedirectToAction("SanPham");
            }
        }
        #endregion
        #region Quản lý ý kiến khách hàng
        //======================================Ý kiến Khách Hàng========================================
        public ActionResult Ykienkhachhang(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_YKienKhachHang"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực này !');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 5;
            return View(data.YKIENKHACHHANGs.ToList().OrderBy(n => n.MAYKIEN).ToPagedList(pageNumber, pageSize));
        }
        //Hien thi y kien
        [HttpGet]
        public ActionResult Xoaykienkhachhang(int id)
        {
            YKIENKHACHHANG ykienkhachhang = data.YKIENKHACHHANGs.SingleOrDefault(n => n.MAYKIEN == id);
            ViewBag.MAYKIEN = ykienkhachhang.MAYKIEN;
            if (ykienkhachhang == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(ykienkhachhang);
        }
        [HttpPost, ActionName("Xoaykienkhachhang")]
        public ActionResult Xacnhanxoa(int id)
        {
            //Lay ra y kien can xoa
            YKIENKHACHHANG ykienkhachhang = data.YKIENKHACHHANGs.SingleOrDefault(n => n.MAYKIEN == id);
            ViewBag.MAYKIEN = ykienkhachhang.MAYKIEN;
            if (ykienkhachhang == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.YKIENKHACHHANGs.DeleteOnSubmit(ykienkhachhang);
            data.SubmitChanges();
            return RedirectToAction("Ykienkhachhang");
        }
        #endregion
        #region Quản lý thương hiệu
        // ==========================================Thương hiệu===========================================
        public ActionResult ThuongHieu(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_ThuongHieu"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực này!');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 9;
            return View(data.THUONGHIEUs.ToList().OrderBy(n => n.MaThuongHieu).ToPagedList(pageNumber, pageSize));
        }
        //THEM THUONG HIEU
        [HttpGet]
        public ActionResult ThemMoiThuongHieu()
        {
            //lay ds tu table THUONGHIEU, sap xep theo Ten thuong hieu, chon lay gia tri MaThuongHieu, hien thi ten thuong hieu
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoiThuongHieu(THUONGHIEU tHUONGHIEU)
        {
            //lay ds tu table THUONGHIEU, sap xep theo Ten thuong hieu, chon lay gia tri MaThuongHieu, hien thi ten thuong hieu
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            data.THUONGHIEUs.InsertOnSubmit(tHUONGHIEU);
            //save vao csdl
            data.SubmitChanges();
            return RedirectToAction("ThuongHieu");
        }
        //xoa thuong hieu
        [HttpGet]
        public ActionResult XoaThuongHieu(int id)
        {
            //lay san pham can xoa
            THUONGHIEU tHUONGHIEU = data.THUONGHIEUs.SingleOrDefault(n => n.MaThuongHieu == id);
            ViewBag.MaThuongHieu = tHUONGHIEU.MaThuongHieu;
            if (tHUONGHIEU == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(tHUONGHIEU);
        }
        [HttpPost, ActionName("XoaThuongHieu")]
        public ActionResult XacNhanXoaThuongHieu(int id)
        {
            THUONGHIEU tHUONGHIEU = data.THUONGHIEUs.SingleOrDefault(n => n.MaThuongHieu == id);
            ViewBag.MaThuongHieu = tHUONGHIEU.MaThuongHieu;
            if (tHUONGHIEU == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.THUONGHIEUs.DeleteOnSubmit(tHUONGHIEU);
            data.SubmitChanges();
            return RedirectToAction("ThuongHieu");
        }
        //sua thong tin thuong hieu
        [HttpGet]
        public ActionResult SuaThuonghieu(int id)
        {
            THUONGHIEU tHUONGHIEU = data.THUONGHIEUs.SingleOrDefault(n => n.MaThuongHieu == id);
            if (tHUONGHIEU == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //dua du lieu vao drop downlist TenTHuongHieu, TenLoai, TenNCC
            ViewBag.MaThuongHieu = new SelectList(data.THUONGHIEUs.ToList().OrderBy(n => n.TenThuongHieu), "MaThuongHieu", "TenThuongHieu");
            return View(tHUONGHIEU);
        }
        [HttpPost, ActionName("SuaThuongHieu")]
        public ActionResult CapNhatThuongHieu(int id)
        {
            THUONGHIEU thuonghieu = data.THUONGHIEUs.SingleOrDefault(n => n.MaThuongHieu == id);
            UpdateModel(thuonghieu);
            data.SubmitChanges();
            return RedirectToAction("ThuongHieu");
        }
        #endregion
        #region Quản lý nhà cung cấp
        // ================================================Nhà cung Cấp===========================
        public ActionResult NhaCungCap(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_NhaCungCap"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực này!');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 9;

            return View(data.NHACUNGCAPs.ToList().OrderBy(n => n.MaNCC).ToPagedList(pageNumber, pageSize));
        }
        #region Trạng thái nhà cung cấp
        [HttpPost]
        public void TrangThaiNCC(int id)
        {
            var _DH = (from d in data.NHACUNGCAPs where d.MaNCC == id select d).SingleOrDefault();
            string _Hinh = "";
            if (_DH.TrangThai == true)
            {
                _DH.TrangThai = false;
                _Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _DH.TrangThai = true;
                _Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_DH);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        [HttpGet]
        public ActionResult ThemMoiNhaCungCap()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoiNhaCungCap(NHACUNGCAP ncc)
        {
            if (String.IsNullOrEmpty(ncc.TenNCC))
            {
                ViewData["Error1"] = "Vui lòng điền đầy đủ thông tin";
                return ThemMoiNhaCungCap();
            }
            if (String.IsNullOrEmpty(ncc.DiaChi))
            {
                ViewData["Error2"] = "Vui lòng điền đầy đủ thông tin";
                return ThemMoiNhaCungCap();
            }

            if (ncc.DienThoai.Length != 10)
            {
                ViewData["Error3"] = "Số điện thoại không đúng định dạng";
                return ThemMoiNhaCungCap();
            }
            if (ncc.TrangThai == null)
            {
                ViewData["Error4"] = "Chọn trạng thái";
                return ThemMoiNhaCungCap();
            }

            data.NHACUNGCAPs.InsertOnSubmit(ncc);
            //save vao csdl
            data.SubmitChanges();
            return RedirectToAction("NhaCungCap");
        }
        [HttpGet]
        public ActionResult XoaNhaCungCap(int id)
        {
            NHACUNGCAP ncc = data.NHACUNGCAPs.SingleOrDefault(n => n.MaNCC == id);
            ViewBag.MaThuongHieu = ncc.MaNCC;
            if (ncc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(ncc);
        }
        [HttpPost, ActionName("XoaNhaCungCap")]
        public ActionResult XacNhanXoaNCC(int id)
        {
            NHACUNGCAP ncc = data.NHACUNGCAPs.SingleOrDefault(n => n.MaNCC == id);
            ViewBag.MaNCC = ncc.MaNCC;
            if (ncc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.NHACUNGCAPs.DeleteOnSubmit(ncc);
            data.SubmitChanges();
            return RedirectToAction("NhaCungCap");
        }
        [HttpGet]
        public ActionResult SuaNhaCungCap(int id)
        {
            NHACUNGCAP ncc = data.NHACUNGCAPs.SingleOrDefault(n => n.MaNCC == id);
            ViewBag.MaNCC = ncc.MaNCC;
            if (ncc == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(ncc);
        }
        [HttpPost, ActionName("SuaNhaCungCap")]
        public ActionResult CapNhatNCC(int id)
        {
            NHACUNGCAP ncc = data.NHACUNGCAPs.SingleOrDefault(n => n.MaNCC == id);
            UpdateModel(ncc);
            data.SubmitChanges();
            return RedirectToAction("NhaCungCap");
        }
        #endregion
        #region Quản lý khách hàng
        // ================================================Quản lý khách hàng===========================
        #region hiện thông tin khách hàng
        public ActionResult KhachHang(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_KhachHang"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực này!');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 9;
            return View(data.KHACHHANGs.ToList().OrderBy(n => n.MaKH).ToPagedList(pageNumber, pageSize));
        }
        #endregion
        #region Quản lý trạng thái khách hàng
        //Hàm khóa hoặc mở khóa tài khoản Admin (ở đây sử dụng hàm void để Response.Write hình update lại)
        [HttpPost]
        public void UpdateTrangThaiKhachHang(int id)
        {
            var _KH = (from kh in data.KHACHHANGs where kh.MaKH == id select kh).SingleOrDefault();
            string Hinh = "";

            if (_KH.TrangThai == true)
            {
                _KH.TrangThai = false;
                Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _KH.TrangThai = true;
                Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_KH);
            data.SubmitChanges();
            Response.Write(Hinh);
        }
        #endregion
        #region Chi tiết khách hàng
        //chi tiet khach hang
        public ActionResult ChiTietKhachHang(int id)
        {
            // lay thuong hieu theo ma th
            KHACHHANG kHACHHANG = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = kHACHHANG.MaKH;
            if (kHACHHANG == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(kHACHHANG);
        }
        #endregion
        #endregion
        #region Quản lý loại giày
        // ================================================Quản lý loai giay===========================
        public ActionResult LoaiGiay(int? page)
        {
            if (Session["Username_Admin"] == null)
                return RedirectToAction("Login");
            else
            if (bool.Parse(Session["PQ_LoaiGiay"].ToString()) == false)//Không đủ quyền hạn
            {
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực quản lý sản phẩm !');window.location='/Admin/';</script>");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 9;
            return View(data.LOAIGIAYs.ToList().OrderBy(n => n.MaLoai).ToPagedList(pageNumber, pageSize));
        }
        #region Trạng thái loại giày
        [HttpPost]
        public void TrangThaiLoai(int id)
        {
            var _DH = (from d in data.LOAIGIAYs where d.MaLoai == id select d).SingleOrDefault();
            string _Hinh = "";
            if (_DH.TrangThai == true)
            {
                _DH.TrangThai = false;
                _Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _DH.TrangThai = true;
                _Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_DH);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        [HttpGet]
        public ActionResult SuaLoaiGiay(int id)
        {
            LOAIGIAY lOAIGIAY = data.LOAIGIAYs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = lOAIGIAY.MaLoai;
            if (lOAIGIAY == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(lOAIGIAY);
        }
        [HttpPost, ActionName("SuaLoaiGiay")]
        public ActionResult SuaLoaiGiayy(int id)
        {
            LOAIGIAY lOAIGIAY = data.LOAIGIAYs.SingleOrDefault(n => n.MaLoai == id);
            UpdateModel(lOAIGIAY);
            data.SubmitChanges();
            return RedirectToAction("LoaiGiay");
        }
        //them thuong hieu
        [HttpGet]
        public ActionResult ThemMoiLoaiGiay()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoiLoaiGiay(LOAIGIAY lOAIGIAY)
        {
            data.LOAIGIAYs.InsertOnSubmit(lOAIGIAY);
            //save vao csdl
            data.SubmitChanges();
            return RedirectToAction("LoaiGiay");
        }
        //xoa loai giay
        [HttpGet]
        public ActionResult XoaLoaiGiay(int id)
        {
            LOAIGIAY lOAIGIAY = data.LOAIGIAYs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = lOAIGIAY.MaLoai;
            if (lOAIGIAY == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(lOAIGIAY);
        }
        [HttpPost, ActionName("XoaLoaiGiay")]
        public ActionResult XacNhanXoaLoaiGiay(int id)
        {
            LOAIGIAY lOAIGIAY = data.LOAIGIAYs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = lOAIGIAY.MaLoai;
            if (lOAIGIAY == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.LOAIGIAYs.DeleteOnSubmit(lOAIGIAY);
            data.SubmitChanges();
            return RedirectToAction("LoaiGiay");
        }
        #endregion
        #region Quản lý đơn hàng
        #region Danh sách đơn hàng
        public ActionResult Order(int? page)
        {
            if (Session["Username_Admin"] == null)//Chưa đăng nhập 
                return RedirectToAction("Login");
            else
                if (bool.Parse(Session["PQ_DonHang"].ToString()) == false)
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực quản trị Đơn Hàng !');window.location='/Admin/';</script>");

            int PageSize = 10;//Chỉ lấy ra 10 dòng (10 đơn hàng )
            int PageNum = (page ?? 1);

            //Lấy ra Danh sách đơn hàng
            var _DH = (from d in data.DONHANGs
                       orderby d.NgayDat descending
                       select d).ToPagedList(PageNum, PageSize);
            return View(_DH);
        }
        #endregion
        #region UpdateOrder
        [HttpPost]
        public void UpdateOrder(int id)
        {
            var _DH = (from d in data.DONHANGs where d.MaDonHang == id select d).SingleOrDefault();
            string _Hinh = "";
            if (_DH.TinhTrangGiaoHang == true)
            {
                _DH.TinhTrangGiaoHang = false;
                _Hinh = "/images/Admin/Icons/icon_An.png";
            }
            else
            {
                _DH.TinhTrangGiaoHang = true;
                _Hinh = "/images/Admin/Icons/icon_Hien.png";
            }
            UpdateModel(_DH);
            data.SubmitChanges();
            Response.Write(_Hinh);
        }
        #endregion
        #region Bảng Chi tiết đơn hàng (OrderDetail)
        public ActionResult OrderDetail(int id)
        {
            if (Session["Username_Admin"] == null)//Chưa đăng nhập 
                return RedirectToAction("Login");
            else
                if (bool.Parse(Session["PQ_DonHang"].ToString()) == false)//Không đủ quyền hạn vào ku vực này 
                return Content("<script>alert('Bạn không đủ quyền hạn vào khu vực này!');window.location='/Admin/';</script>");
            var CT_DH = (from c in data.CT_DONHANGs
                         where c.MaDonHang == id
                         select c).ToList();
            return View(CT_DH);
        }
        #endregion
        #endregion



    }
}