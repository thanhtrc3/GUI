using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BUS;
using DAL.MODELS;

namespace GUI
{
    public partial class frmDK : Form
    {
        private FacultyService facultyService = new FacultyService();
        private MajorService majorService = new MajorService();
        private StudentService studentService = new StudentService();

        private frmQLSV parentForm;   // form cha để refresh sau khi đăng ký

        // Nhận tham chiếu frmQLSV từ form cha
        public frmDK(frmQLSV parent)
        {
            InitializeComponent();
            parentForm = parent;
        }

        private void frmDK_Load(object sender, EventArgs e)
        {
            LoadFacultyCombobox();
        }

        // ====== Nạp danh sách khoa ======
        private void LoadFacultyCombobox()
        {
            var listFaculty = facultyService.GetAll();

            // Thêm dòng mặc định
            listFaculty.Insert(0, new Faculty
            {
                FacultyID = 0,
                FacultyName = "-- Chọn khoa --"
            });

            cmbKhoa.DataSource = listFaculty;
            cmbKhoa.DisplayMember = "FacultyName";
            cmbKhoa.ValueMember = "FacultyID";
            cmbKhoa.SelectedIndex = 0;

            cmbChuyenNganh.DataSource = null;
            dgvChuyenNganh.Rows.Clear();
        }

        // ====== Hiển thị danh sách sinh viên chưa có chuyên ngành ======
        private void BindGrid(List<Student> listStudent)
        {
            dgvChuyenNganh.Rows.Clear();

            if (listStudent == null || listStudent.Count == 0)
                return;

            foreach (var item in listStudent)
            {
                int idx = dgvChuyenNganh.Rows.Add();
                var row = dgvChuyenNganh.Rows[idx];

                row.Cells["colSelect"].Value = false;
                row.Cells["colStudentID"].Value = item.StudentID ?? "";
                row.Cells["colFullName"].Value = item.FullName ?? "";
                row.Cells["colFaculty"].Value = item.Faculty?.FacultyName ?? "";
                row.Cells["colAverageScore"].Value = item.AverageScore.ToString("0.00");
            }
        }

        // ====== Khi chọn Khoa ======
        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {
            var faculty = cmbKhoa.SelectedItem as Faculty;

            if (faculty == null || faculty.FacultyID == 0)
            {
                cmbChuyenNganh.DataSource = null;
                dgvChuyenNganh.Rows.Clear();
                return;
            }

            // Lấy danh sách chuyên ngành thuộc khoa được chọn
            var listMajor = majorService.GetAllByFaculty(faculty.FacultyID);

            if (listMajor == null || listMajor.Count == 0)
            {
                cmbChuyenNganh.DataSource = null;
            }
            else
            {
                listMajor.Insert(0, new Major
                {
                    MajorID = 0,
                    Name = "-- Chọn chuyên ngành --",
                    FacultyID = faculty.FacultyID
                });

                cmbChuyenNganh.DataSource = listMajor;
                cmbChuyenNganh.DisplayMember = "Name";
                cmbChuyenNganh.ValueMember = "MajorID";
                cmbChuyenNganh.SelectedIndex = 0;
            }

            // Hiển thị danh sách sinh viên của khoa chưa có chuyên ngành
            var listStudent = studentService.GetAllHasNoMajor(faculty.FacultyID);
            BindGrid(listStudent);
        }

        // ====== Khi chọn chuyên ngành ======
        private void cmbChuyenNganh_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Sự kiện này chỉ cần nhận giá trị chuyên ngành, không cần làm gì thêm
        }

        // ====== Khi nhấn nút Đăng ký ======
        private void btnRegister_Click(object sender, EventArgs e)
        {
            var faculty = cmbKhoa.SelectedItem as Faculty;
            if (faculty == null || faculty.FacultyID == 0)
            {
                MessageBox.Show("Vui lòng chọn khoa hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var major = cmbChuyenNganh.SelectedItem as Major;
            if (major == null || major.MajorID == 0)
            {
                MessageBox.Show("Vui lòng chọn chuyên ngành hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int successCount = 0;

            foreach (DataGridViewRow row in dgvChuyenNganh.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells["colSelect"].Value);
                if (!isChecked) continue;

                string studentId = row.Cells["colStudentID"].Value?.ToString();
                if (string.IsNullOrEmpty(studentId)) continue;

                var student = studentService.FindById(studentId);
                if (student != null && student.FacultyID == faculty.FacultyID)
                {
                    student.MajorID = major.MajorID;
                    studentService.InsertUpdate(student);
                    successCount++;
                }
            }

            if (successCount == 0)
            {
                MessageBox.Show("Không có sinh viên nào được đăng ký.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"Đã đăng ký thành công cho {successCount} sinh viên.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Làm mới danh sách sinh viên chưa có chuyên ngành
            var listStudent = studentService.GetAllHasNoMajor(faculty.FacultyID);
            BindGrid(listStudent);

            // Reset combobox chuyên ngành
            cmbChuyenNganh.SelectedIndex = 0;

            // Cập nhật lại DataGridView bên frmQLSV
            parentForm?.RefreshStudentGrid();
        }

        

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
