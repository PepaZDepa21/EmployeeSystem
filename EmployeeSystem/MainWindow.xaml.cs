using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace EmployeeSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Employee emp = new Employee() { Birthdate = DateTime.Now };
        public MainWindow()
        {
            InitializeComponent();
            DataContext = emp;
            Employee.GetEmployeesFromFile();
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            if (emp.IsOk())
            {
                Employee temp = Employee.AllEmployees.Find(em => em.ID == emp.ID);
                int empIndex = Employee.AllEmployees.IndexOf(temp);

                if (temp != null)
                {
                    Employee.AllEmployees[empIndex] = Employee.CopyEmployee(emp);
                }
                else
                {
                    if (emp.NotEnteredBirthdate())
                    {
                        emp.Birthdate = null;
                    }
                    Employee.AllEmployees.Add(Employee.CopyEmployee(emp));
                    emp.Clear();
                }


                Employee.WriteAllEmployeesToFile();

                UpdateListView();
            }
            else
            {
                MessageBox.Show("Some values are inappropriate!!!");
            }
        }

        private void BtnClearClick(object sender, RoutedEventArgs e)
        {
            emp.Clear();
        }

        private void BtnEditEmployeeClick(object sender, RoutedEventArgs e)
        {
            emp.Firstname = (((Button) sender).DataContext as Employee).Firstname;
            emp.Lastname = (((Button) sender).DataContext as Employee).Lastname;
            emp.Birthdate = (((Button) sender).DataContext as Employee).Birthdate;
            emp.Education = (((Button) sender).DataContext as Employee).Education;
            emp.Position = (((Button) sender).DataContext as Employee).Position;
            emp.GrossSalary = (((Button) sender).DataContext as Employee).GrossSalary;
            emp.ID = (((Button) sender).DataContext as Employee).ID;
        }

        private void BtnDeleteEmployeeClick(object sender, RoutedEventArgs e)
        {
            Employee em = ((Button)sender).DataContext as Employee;
            Employee.AllEmployees.Remove(em);

            UpdateListView();
        }

        public void UpdateListView()
        {
            lvEmps.ItemsSource = null;
            lvEmps.ItemsSource = Employee.AllEmployees;
        }
    }

    class Person : INotifyPropertyChanged
    {
        private string? firstname, lastname;
        private DateTime? birthdate;
        protected Regex name = new Regex("^([A-ZŠČŘŽÝÁÍÉÓÚŮĎŤŇ]{1}([a-zěščřžýáíéóúůďťň]{1,9})?)([- ']{1}[A-ZŠČŘŽÝÁÍÉÓÚŮĎŤŇ]{1}[a-zěščřžýáíéóúůďťň']{3,9})?$");

        private string firstnameError, lastnameError, birthdateError;

        [JsonIgnore]
        public string FirstnameError
        {
            get { return firstnameError; }
            set { firstnameError = value; }
        }
        [JsonIgnore]
        public string LastnameError 
        { 
            get { return lastnameError; }
            set { lastnameError = value; }
        }
        [JsonIgnore]
        public string BirthdateError
        {
            get { return birthdateError; }
            set { birthdateError = value; }
        }

        public string? Firstname {
            get
            {
                //if (firstname == null)
                //{
                //    return "";
                //}
                return firstname;
            }
            set
            {
                if (value == null || name.IsMatch(value))
                {
                    firstname = value;
                    FirstnameError = string.Empty;
                    OnPropertyChanged("Firstname");
                    OnPropertyChanged("FirstnameError");
                    OnPropertyChanged("Status");
                }
                else {
                    firstname = value;
                    FirstnameError = "Firstname doesn't match pattern!!!";
                    OnPropertyChanged("FirstnameError");
                    OnPropertyChanged("Status");
                }
            }
        }

        public string? Lastname
        {
            get { return lastname; }
            set
            {
                if (value == null || name.IsMatch(value))
                {
                    lastname = value;
                    LastnameError = string.Empty;
                    OnPropertyChanged("Lastname");
                    OnPropertyChanged("LastnameError");
                    OnPropertyChanged("Status");
                }
                else
                {
                    lastname = value;
                    LastnameError = "Lastname doesn't match pattern!!!";
                    OnPropertyChanged("LastnameError");
                    OnPropertyChanged("Status");
                }
            }
        }

        public DateTime? Birthdate
        {
            get { return birthdate; }
            set
            {
                if (value == null)
                {
                    birthdate = value;
                    OnPropertyChanged("Status");
                }
                else
                {
                    birthdate = value;
                    DateTime db = (DateTime)value;
                    if (CheckAge() || NotEnteredBirthdate())
                    {
                        BirthdateError = string.Empty;
                        OnPropertyChanged("Birthdate");
                        OnPropertyChanged("BirthdateError");
                        OnPropertyChanged("Status");
                    }
                    else
                    {
                        BirthdateError = "Person is too young for the job!!!";
                        OnPropertyChanged("BirthdateError");
                        OnPropertyChanged("Status");
                    }
                }
            }
        }

        public bool CheckAge()
        {
            DateTime dt = DateTime.Now;
            DateTime bd;
            if (Birthdate == null)
            {
                return true;
            }
            else
            {
                bd = (DateTime)Birthdate;
            }
            return dt.Year - bd.Year > 15 || (dt.Year - bd.Year == 15 && dt.DayOfYear >= bd.DayOfYear);
        }

        public bool NotEnteredBirthdate()
        {
            DateTime dt = DateTime.Now;
            DateTime bd;
            if (Birthdate == null)
            {
                return true;
            }
            else
            {
                bd = (DateTime)Birthdate;
            }
            return dt.Year == bd.Year && dt.DayOfYear == bd.DayOfYear;
        }

        public override string ToString()
        {
            return $"{{\"Firstname\":{Firstname},\"Lastname\":{Lastname},\"Birthdate\":{Birthdate}}}";
        }

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    class Employee : Person
    {
        private string? education { get; set; }
        private string? position { get; set; }
        private decimal grossSalary { get; set; }

        [JsonIgnore]
        public Guid ID { get; set; }

        [JsonIgnore]
        public static List<Employee> AllEmployees { get; set; } = new List<Employee>();

        private string educationError, positionError, grossSalaryError;

        [JsonIgnore]
        public string EducationError
        {
            get { return educationError; }
            set { educationError = value; }
        }

        [JsonIgnore]
        public string PositionError
        {
            get { return positionError; }
            set { positionError = value; }
        }

        [JsonIgnore]
        public string GrossSalaryError
        {
            get { return grossSalaryError; }
            set { grossSalaryError = value; }
        }

        public string? Education
        {
            get { return education; }
            set
            {
                if (Education != "" || Education == null)
                {
                    education = value;
                    EducationError = string.Empty;
                    OnPropertyChanged("Education");
                    OnPropertyChanged("EducationError");
                    OnPropertyChanged("Status");
                }
                else
                {
                    EducationError = "Unknown or empty education!!!";
                    OnPropertyChanged("EducationError");
                    OnPropertyChanged("Status");
                }
            }
        }

        public string? Position
        {
            get { return position; }
            set
            {
                if (Position != "" || Position == null)
                {
                    position = value;
                    PositionError = string.Empty;
                    OnPropertyChanged("Position");
                    OnPropertyChanged("PositionError");
                    OnPropertyChanged("Status");
                }
                else
                {
                    PositionError = "Position not entered or unknown posiotion!!!";
                    OnPropertyChanged("PositionError");
                    OnPropertyChanged("Status");
                }
            }
        }

        public decimal GrossSalary
        {
            get { return grossSalary; }
            set
            {
                if ((decimal) value >= (decimal) 17300)
                {
                    grossSalary = value;
                    GrossSalaryError = string.Empty;
                    OnPropertyChanged("GrossSalary");
                    OnPropertyChanged("GrossSalaryError");
                    OnPropertyChanged("Status");
                }
                else if (value == 0)
                {
                    grossSalary = 0;
                    GrossSalaryError = string.Empty;
                    OnPropertyChanged("GrossSalary");
                    OnPropertyChanged("GrossSalaryError");
                    OnPropertyChanged("Status");
                }
                else
                {
                    grossSalary = value;
                    GrossSalaryError = "Salary doesn't match minimal wage";
                    OnPropertyChanged("GrossSalaryError");
                    OnPropertyChanged("Status");
                }
            }
        }

        [JsonIgnore]
        public string Status
        {
            get => this.ToString();
        }

        public bool IsOk()
        {
            bool fn;
            bool ln;
            bool bd;
            if (Firstname == null)
            {
                fn = true;
            }
            else
            {
                fn = name.IsMatch(Firstname);
            }
            if (Lastname == null)
            {
                ln = false;
            }
            else
            {
                ln = name.IsMatch(Lastname);
            }
            if (Birthdate == null)
            {
                bd = true;
            }
            else
            {
                bd = CheckAge() || NotEnteredBirthdate();
            }
            bool po = Position != null && Position != "";
            bool gs = GrossSalary >= (decimal) 17300;
            return fn && ln && bd && po && gs;
        }

        public override string ToString()
        {
            if (!NotEnteredBirthdate())
            {
                return $"{{\"Firstname\":\"{Firstname}\",\"Lastname\":\"{Lastname}\",\"Birthdate\":{Birthdate},\"Education\":\"{Education}\",\"Position\":\"{Position}\",\"GrossSalary\":{GrossSalary}}}";
            }
            return $"{{\"Firstname\":\"{Firstname}\",\"Lastname\":\"{Lastname}\",\"Birthdate\":null,\"Education\":\"{Education}\",\"Position\":\"{Position}\",\"GrossSalary\":{GrossSalary}}}";
        }

        public static void WriteAllEmployeesToFile()
        {
            string path = @".\Employees.txt";
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var item in AllEmployees)
                {
                    sw.WriteLine(SerializeClassToJSON(item));
                }
                sw.Flush();
            }
        }

        public static void GetEmployeesFromFile()
        {
            string path = @".\Employees.txt";
            using (StreamReader sr = new StreamReader(path))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null) { break; }
                    var emp = JsonSerializer.Deserialize<Employee>(line);
                    AllEmployees.Add(emp);
                }
            }
        }

        public static string SerializeClassToJSON(Employee employee) 
        {
            return JsonSerializer.Serialize(employee);
        }

        public static Employee CopyEmployee(Employee empl) 
        {
            Employee em = new Employee();
            em.Firstname = empl.Firstname;
            em.Lastname = empl.Lastname;
            em.Birthdate = empl.Birthdate;
            em.Education = empl.Education;
            em.Position = empl.Position;
            em.GrossSalary = empl.GrossSalary;
            em.ID = Guid.NewGuid();
            return em;
        }

        public void Clear()
        {
            Firstname = null;
            Lastname = null;
            Birthdate = DateTime.Now;
            Education = null;
            Position = null;
            GrossSalary = 0;
        }
    }
}