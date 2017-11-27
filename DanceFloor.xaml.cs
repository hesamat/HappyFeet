using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HappyFeet
{
    /// <summary>
    /// Interaction logic for controlPanel.xaml
    /// </summary>
    public partial class DanceFloor : Window, INotifyPropertyChanged
    {
        private const double INITIAL_Q_X = 0;
        private const double INITIAL_Q_Y = 1;
        private const double INITIAL_Q_Z = 0;
        private const double INITIAL_Q_W = 0;

        private const double INITIAL_PLANE_ANGLE = 85;
        private const double INITIAL_OPACITY = 0.4;

        private double q_x_Value;
        private double q_y_Value;
        private double q_z_Value;
        private double q_w_Value;

        private double shoe_plane_value;
        private double grid_width_value;
        private double grid_length_value;
        private double grid_height_value;
        private double grid_opacity_value;


        public double ShoePlane
        {
            get { return shoe_plane_value; }
            set
            {
                shoe_plane_value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ShoePlane"));
                }
            }
        }

        public double GridHeight
        {
            get { return grid_height_value; }
            set
            {
                grid_height_value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GridHeight"));
                }
            }
        }

        public double GridWidth
        {
            get { return grid_width_value; }
            set
            {
                grid_width_value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GridWidth"));
                }
            }
        }

        public double GridLength
        {
            get { return grid_length_value; }
            set
            {
                grid_length_value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GridLength"));
                }
            }
        }

        public double GridOpacity
        {
            get { return grid_opacity_value; }
            set
            {
                grid_opacity_value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GridOpacity"));
                }
            }
        }

        public double Quaternion_X
        {
            get { return q_x_Value; }
            set
            {
                q_x_Value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Quaternion_X"));
                }
            }
        }
        public double Quaternion_Y
        {
            get { return q_y_Value; }
            set
            {
                q_y_Value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Quaternion_Y"));
                }
            }
        }
        public double Quaternion_Z
        {
            get { return q_z_Value; }
            set
            {
                q_z_Value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Quaternion_Z"));
                }
            }
        }
        public double Quaternion_W
        {
            get { return q_w_Value; }
            set
            {
                q_w_Value = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Quaternion_W"));
                }
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public DanceFloor()
        {
            InitializeComponent();
            Quaternion_X = INITIAL_Q_X;
            Quaternion_Y = INITIAL_Q_Y;
            Quaternion_Z = INITIAL_Q_Z;
            Quaternion_W = INITIAL_Q_W;
            this.WindowState = WindowState.Maximized;

            ShoePlane = INITIAL_PLANE_ANGLE;            
            GridOpacity = INITIAL_OPACITY;
            
            //localDancerPlane.Material = new SolidColorBrush()
            //this.WindowStyle = WindowStyle.None;

            //System.Windows.Application.Current.Windows.WindowState = WindowState.Maximized;
            //System.Windows.Application.Current.MainWindow.WindowStyle = WindowStyle.None;    
            
        }

    }
}
