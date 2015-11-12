using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Logica;

namespace T4_Jose_Montes
{
    /// <summary>
    /// Interaction logic for UCWorm.xaml
    /// </summary>
    public partial class UCWorm : UserControl
    {
        public Worm w;
        public double yspeed = 0;
        public double xspeed = 0;
        public double CanvasXPos;
        public double CanvasYPos;
        public bool onAir = false;
        
        public UCWorm(Worm _w)
        {
            InitializeComponent();
            this.w = _w;
            nombre.Content = _w.nombre;
            nombre.Foreground = (_w.equipo == Bandos.bando.rojo) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Blue);
            nombre.FontSize = 14;
            hp.Content = _w.hp;
            hp.Foreground = (_w.equipo == Bandos.bando.rojo) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Blue);
            hp.FontSize = 14;
        }

    }
}
