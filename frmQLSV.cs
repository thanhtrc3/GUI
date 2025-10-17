using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BUS;
using DAL.MODELS;

namespace GUI
{
    public partial class frmQLSV : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private string avatarFilePath = string.Empty;

        public frmQLSV()
        {
            InitializeComponent();
        }

        private void frmQLSV_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dgvStudent);
                FillFacultyCombobox(facultyService.GetAll());
                BindGrid(studentService.GetAll());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void FillFacultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty { FacultyID = 0, FacultyName = "-- Chọn Khoa --" });
            cmbKhoa.DataSource = listFacultys;
            cmbKhoa.DisplayMember = "FacultyName";
            cmbKhoa.ValueMember = "FacultyID";
            cmbKhoa.SelectedIndex = 0;
        }

        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var s in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = s.StudentID;
                dgvStudent.Rows[index].Cells[1].Value = s.FullName;
                dgvStudent.Rows[index].Cells[2].Value = s.Faculty?.FacultyName;
                dgvStudent.Rows[index].Cells[3].Value = s.AverageScore.ToString("0.00");
                dgvStudent.Rows[index].Cells[4].Value = s.Major?.Name;
            }
        }

        private void ShowAvatar(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                picAvatar.Image = null;
                return;
            }

            string folderPath = Path.Combine(Application.StartupPath, "Images");
            string imagePath = Path.Combine(folderPath, imageName);

            if (File.Exists(imagePath))
                picAvatar.Image = Image.FromFile(imagePath);
            else
                picAvatar.Image = null;
        }

        private void ClearData()
        {
            txtMSSV.Clear();
            txtHoTen.Clear();
            txtDTB.Clear();
            cmbKhoa.SelectedIndex = 0;
            picAvatar.Image = null;
            avatarFilePath = string.Empty;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMSSV.Text) ||
                    string.IsNullOrWhiteSpace(txtHoTen.Text) ||
                    string.IsNullOrWhiteSpace(txtDTB.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!double.TryParse(txtDTB.Text, out double dtb) || dtb < 0 || dtb > 10)
                {
                    MessageBox.Show("Điểm trung bình không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbKhoa.SelectedValue == null || (int)cmbKhoa.SelectedValue == 0)
                {
                    MessageBox.Show("Vui lòng chọn khoa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var existing = studentService.FindById(txtMSSV.Text.Trim());
                bool isNew = existing == null;

                Student sv = isNew ? new Student() : existing;
                sv.StudentID = txtMSSV.Text.Trim();
                sv.FullName = txtHoTen.Text.Trim();
                sv.AverageScore = dtb;
                sv.FacultyID = (int)cmbKhoa.SelectedValue;

                if (!string.IsNullOrEmpty(avatarFilePath))
                    sv.Avatar = SaveAvatar(avatarFilePath, sv.StudentID);

                studentService.InsertUpdate(sv);

                MessageBox.Show(isNew ? "Thêm thành công!" : "Cập nhật thành công!");
                BindGrid(studentService.GetAll());
                ClearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private string SaveAvatar(string path, string studentID)
        {
            string folder = Path.Combine(Application.StartupPath, "Images");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string dest = Path.Combine(folder, $"{studentID}{Path.GetExtension(path)}");
            File.Copy(path, dest, true);
            return Path.GetFileName(dest);
        }

        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string id = dgvStudent.Rows[e.RowIndex].Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;

            var s = studentService.FindById(id);
            if (s == null) return;

            txtMSSV.Text = s.StudentID;
            txtHoTen.Text = s.FullName;
            txtDTB.Text = s.AverageScore.ToString("0.00");
            cmbKhoa.SelectedValue = s.FacultyID;
            ShowAvatar(s.Avatar);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvStudent.SelectedRows.Count == 0) return;

            string id = dgvStudent.SelectedRows[0].Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                studentService.Delete(id);
                BindGrid(studentService.GetAll());
            }
        }

        

        public void RefreshStudentGrid()
        {
            BindGrid(studentService.GetAll());
        }

        private void ChuyenNganhTSMI_Click(object sender, EventArgs e)
        {
            frmDK frm = new frmDK(this);
            frm.FormClosed += (s, args) => this.Show();
            frm.Show();
            this.Hide();
        }

        private void ChuyenNganhTSB_Click(object sender, EventArgs e)
        {
            ChuyenNganhTSMI_Click(sender, e);
        }

        
        

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ThoatTSMI_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkChuaDkChuyenNganh_CheckedChanged(object sender, EventArgs e)
        {
            var list = new List<Student>();
            if(chkChuaDkChuyenNganh.Checked)
                list = studentService.GetAllHasNoMajor();
            else
                list = studentService.GetAll();
            BindGrid(list);
        }
    }
}
