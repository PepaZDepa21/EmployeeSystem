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

namespace EmployeeSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Employee emp;
        public MainWindow()
        {
            emp= new Employee() { Birthdate = DateTime.Now};
            InitializeComponent();
            DataContext = emp;
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            string path = @".\Employees.txt";
            string textToAppend = JsonSerializer.Serialize(emp);

            //https://stackoverflow.com/questions/9092160/check-if-a-folder-exist-in-a-directory-and-create-them-using-c-sharp
            if (File.Exists(path))
            {
                //https://stackoverflow.com/questions/2837020/open-existing-file-append-a-single-line
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(textToAppend);
                    sw.Flush();
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(textToAppend);
                    sw.Flush();
                }
            }
            MessageBox.Show(emp.ToString());
            emp.Clear();
        }

        private void BtnClearClick(object sender, RoutedEventArgs e)
        {
            emp.Clear();
        }
    }

    class Person : INotifyPropertyChanged
    {
        private string? firstname, lastname;
        private DateTime? birthdate;

        public string? Firstname {
            get { return firstname; }
            set
            {
                firstname = value;
                OnPropertyChanged("Firstname");
            }
        }

        public string? Lastname
        {
            get { return lastname; }
            set
            {
                lastname = value;
                OnPropertyChanged("Lastname");
            }
        }

        public DateTime? Birthdate
        {
            get { return birthdate; }
            set
            {
                birthdate = value;
                OnPropertyChanged("Birthdate");
            }
        }

        public override string ToString()
        {
            return $"{Firstname} {Lastname} {Birthdate}";
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
        private decimal? grossSalary { get; set; }
        
        public string? Education
        {
            get { return education; }
            set
            {
                education = value;
                OnPropertyChanged("Education");
            }
        }

        public string? Position
        {
            get { return position; }
            set
            {
                position = value;
                OnPropertyChanged("Position");
            }
        }

        public decimal? GrossSalary
        {
            get { return grossSalary; }
            set
            {
                grossSalary = value;
                OnPropertyChanged("GrossSalary");
            }
        }

        public override string ToString()
        {
            return $"{Firstname} {Lastname} {Birthdate} {Education} {Position} {GrossSalary}";
        }

        public void Clear()
        {
            Firstname = string.Empty;
            Lastname = string.Empty;
            Birthdate = DateTime.Now;
            Education = string.Empty;
            Position = string.Empty;
            GrossSalary = 0;
        }
    }
}