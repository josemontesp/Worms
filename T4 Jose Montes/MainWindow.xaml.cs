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
using WpfAnimatedGif;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using Logica;

namespace T4_Jose_Montes
{
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {//http://geekswithblogs.net/NewThingsILearned/archive/2008/08/25/refresh--update-wpf-controls.aspx
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }

        public static Queue<UCWorm> RemoveFromQueue(UCWorm item, Queue<UCWorm> queue)
        {

            var list = queue.ToList();
            list.Remove(item);
            var finalQueue = new Queue<UCWorm>();
            foreach (UCWorm i in list)
            {
                finalQueue.Enqueue(i);
            }
            return finalQueue;
            
        }

    } 
    public partial class MainWindow : Window
    {
        public Core core = new Core();
        public Random randy = new Random();
        public List<UCWorm> worms = new List<UCWorm>();
        public List<Tuple<Image, Proyectil>> proyectiles = new List<Tuple<Image, Proyectil>>();
        public Queue<UCWorm> turnos = new Queue<UCWorm>();
        public Image[,] bloques;
        public int gravedad = 300000;
        public double velocidadDeSalto = -8000;
        DispatcherTimer BazookaControl;
        DispatcherTimer BatControl;
        DispatcherTimer GatilloActualizador;

        public event Action explotado;

        public MainWindow()
        {
            InitializeComponent();
            CargarMapa();
            GatilloActualizador = new DispatcherTimer();
            GatilloActualizador.Interval = new TimeSpan(0, 0, 0, 0, 10);
            GatilloActualizador.Tick += Actualizar;
            GatilloActualizador.Start();

            BazookaControl = new DispatcherTimer();
            BazookaControl.Interval = new TimeSpan(0, 0, 0, 0, 1);
            BazookaControl.Tick += ActualizarBazooka;

            BatControl = new DispatcherTimer();
            BatControl.Interval = new TimeSpan(0, 0, 0, 0, 1);
            BatControl.Tick += ActualizaBat;

            this.WindowState = WindowState.Maximized;
            this.KeyDown += MainWindow_KeyDown;
            bazookabtn.Click += SeleccionarBazooka;
            bat.Click += SeleccionarBat;

            airstrike.Click += airstrike_Click;

            CambiarDeTurno();

            
        }

        void airstrike_Click(object sender, RoutedEventArgs e)
        {
            Proyectil p;
            for (int i = 0; i < 350; i = i + 100)
            {
                p = new Proyectil(-100, 0, turnos.Peek().CanvasXPos + i, 10, 40, 40);
                Image img = new Image();
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\bazooka.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(img, image);
                Canvas.SetTop(img, p.CanvasPosY);
                Canvas.SetLeft(img, p.CanvasPosX);
                MyCanvas.Children.Add(img);

                proyectiles.Add(new Tuple<Image, Proyectil>(img, p));
            }
        }

        void SeleccionarBat(object sender, RoutedEventArgs e)
        {
            potencia.Visibility = System.Windows.Visibility.Visible;
            potencia2.Visibility = System.Windows.Visibility.Visible;
            var worm = turnos.Peek();
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\bat.gif");
            image.EndInit();
            ImageBehavior.SetAnimatedSource(worm.icono, image);
            BatControl.Start();
        }

        void ActualizaBat(object sender, EventArgs e)
        {
            var mousePosX = Mouse.GetPosition(MyCanvas).X;
            var mousePosY = Mouse.GetPosition(MyCanvas).Y - 80;
            var batPosX = turnos.Peek().CanvasXPos+15;
            var batPosY = turnos.Peek().CanvasYPos-30;

            #region Angulo de ataque
            var controller = ImageBehavior.GetAnimationController(turnos.Peek().icono);

            var angle = Math.Atan((mousePosY - batPosY) / (mousePosX - batPosX)) + (Math.PI / 2); // va de 0 a Pi
            var frame = 0.0;
            if (mousePosX < batPosX)
            {
                frame = (angle * 31) / (Math.PI);
                ScaleTransform scale = new ScaleTransform();

                scale.ScaleX = 1;
                scale.CenterX = 45;
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(scale);
                turnos.Peek().icono.RenderTransform = myTransformGroup;
                try
                {
                    controller.GotoFrame((int)frame);
                }
                catch { }
                
            }
            else
            {
                ScaleTransform scale = new ScaleTransform();
                scale.ScaleX = -1;
                scale.CenterX = 45;
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(scale);
                turnos.Peek().icono.RenderTransform = myTransformGroup;
                frame = -1 * (((angle * 31) / (Math.PI)) - 14) + 17;
                controller.GotoFrame((int)frame);
            }
            #endregion
            #region Barra de Potencia
            
            
            var speedx = mousePosX - batPosX;
            var speedy = mousePosY - batPosY;
            var speed = Math.Sqrt(Math.Pow(speedx, 2) + Math.Pow(speedy, 2));
            var barraPotencia = (160.0 / speed) * 950;
            //lb.Content = barraPotencia;
            if (barraPotencia >= 33)
            {
                potencia.Margin = new Thickness(33, 600, barraPotencia, 10);
            }
            #endregion
            #region Batear
            
            
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                potencia.Visibility = System.Windows.Visibility.Hidden;
                potencia2.Visibility = System.Windows.Visibility.Hidden;
                //Dañar worms cercanos
                foreach (UCWorm w in worms)
                {
                    if (w == turnos.Peek())
                        continue;
                    if (Math.Sqrt(Math.Pow(w.CanvasXPos - batPosX, 2) + Math.Pow(w.CanvasYPos - batPosY, 2)) < 60)
                    {
                        w.w.hp -= 20;
                        var xspeed = (mousePosX - batPosX)*10;
                        var yspeed = (mousePosY - batPosY)*10;
                        var maxspeed = 6000;
                        if (Math.Sqrt(Math.Pow(xspeed, 2) + Math.Pow(yspeed, 2)) > maxspeed)
                        {
                            xspeed = (xspeed / Math.Sqrt(Math.Pow(xspeed, 2) + Math.Pow(yspeed, 2))) * maxspeed;
                            yspeed = (yspeed / Math.Sqrt(Math.Pow(xspeed, 2) + Math.Pow(yspeed, 2))) * maxspeed;
                        }
                        w.xspeed = xspeed;
                        w.yspeed = yspeed;
                        
                    }
                }
                BatControl.Stop();
                CambiarDeTurno();

            }
            #endregion
        }

        public void CargarMapa()
        {
            #region Cargar Suelo
            var converter = new ImageSourceConverter();
            var source = (ImageSource)converter.ConvertFromString(@"..\..\..\gifs\block.jpg");
            bloques = new Image[core.m.largo, core.m.alto];
            for (int x = 0; x < core.m.largo; x++)
            {
                //var column = new ColumnDefinition() { Width = new GridLength(30) }; ;
                //MyGrid.ColumnDefinitions.Add(column);
                for (int y = 0; y < core.m.alto; y++)
                {
                    //var row = new RowDefinition() { Height = new GridLength(30) };
                    //MyGrid.RowDefinitions.Add(row);
                    if (core.m.grilla[x, y] == "1")
                    {
                        var img = new Image()
                        {
                            Source = source,
                            Height = 30,
                            Width = 30,
                        };

                        MyCanvas.Children.Add(img);
                        Canvas.SetLeft(img, x * 30);
                        Canvas.SetTop(img, y * 30);
                        bloques[x, y] = img;
                    }
                }
            }
            var transform = 42.0 / core.m.largo;
            MyCanvas.LayoutTransform = new ScaleTransform(transform, transform);
            #endregion

            #region Cargar Worms
            var i = 0;
            foreach (Worm w in core.worms)
            {
                worms.Add(new UCWorm(w));

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\9.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(worms[i].icono, image);

                Canvas.SetLeft(worms[i], w.posicion.x * 30);
                Canvas.SetTop(worms[i], w.posicion.y * 30 - 36);
                worms[i].CanvasXPos = w.posicion.x * 30;
                worms[i].CanvasYPos = w.posicion.y * 30 - 36;
                MyCanvas.Children.Add(worms[i]);

                i++;
            }
            #endregion
            #region Cola de turnos
            foreach (UCWorm w in worms)
                turnos.Enqueue(w);

            #endregion
           
        }

        void ActualizarBazooka(object sender, EventArgs e)
        {

            var mousePosX = Mouse.GetPosition(MyCanvas).X;
            var mousePosY = Mouse.GetPosition(MyCanvas).Y -80;
            var bazookaPosX = turnos.Peek().CanvasXPos+15;
            var bazookaPosY = turnos.Peek().CanvasYPos-30;
            // Angulo de ataque
            var controller = ImageBehavior.GetAnimationController(turnos.Peek().icono);
            
            var angle = Math.Atan((mousePosY - bazookaPosY) / (mousePosX - bazookaPosX)) + (Math.PI/2); // va de 0 a Pi
            var frame = 0.0;
            if (mousePosX < bazookaPosX)
            {
                frame = (angle * 31) / (Math.PI);
                ScaleTransform scale = new ScaleTransform();

                scale.ScaleX = 1;
                scale.CenterX = 45;
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(scale);
                turnos.Peek().icono.RenderTransform = myTransformGroup;
                controller.GotoFrame((int)frame);
            }
            else
            {
                ScaleTransform scale = new ScaleTransform();
                scale.ScaleX = -1;
                scale.CenterX = 45;
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(scale);
                turnos.Peek().icono.RenderTransform = myTransformGroup;
                frame = -1*(((angle * 31) / (Math.PI)) - 14) + 17;
                controller.GotoFrame((int)frame);
            }
            //Barra de potencia
            
            var speedx = mousePosX - bazookaPosX;
            var speedy = mousePosY - bazookaPosY;
            var speed = Math.Sqrt(Math.Pow(speedx, 2) + Math.Pow(speedy, 2));
            var barraPotencia = (160.0/speed)*950 ;
            //lb.Content = barraPotencia;
            if (barraPotencia >= 33)
            {
                potencia.Margin = new Thickness(33, 600, barraPotencia, 10);
            }



            //lb.Content = "frame = " + frame;
            if (Mouse.LeftButton == MouseButtonState.Pressed && proyectiles.Count == 0){
                potencia.Visibility = System.Windows.Visibility.Hidden;
                potencia2.Visibility = System.Windows.Visibility.Hidden;

                Proyectil p = new Proyectil(speedx, speedy, turnos.Peek().CanvasXPos, turnos.Peek().CanvasYPos, 90, 40);
                Image img = new Image();
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\bazooka.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(img, image);
                Canvas.SetTop(img, p.CanvasPosY);
                Canvas.SetLeft(img, p.CanvasPosX);
                MyCanvas.Children.Add(img);
                
                proyectiles.Add(new Tuple<Image, Proyectil>(img, p));
                BazookaControl.Stop();
                
            }
            

        }

        void SeleccionarBazooka(object sender, RoutedEventArgs e)
        {
            if (proyectiles.Count == 0)
            {
                potencia.Visibility = System.Windows.Visibility.Visible;
                potencia2.Visibility = System.Windows.Visibility.Visible;

                var worm = turnos.Peek();
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\16.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(worm.icono, image);
                BazookaControl.Start();
            }
        }

        void Actualizar(object sender, EventArgs e)
        {
            #region ControlDeProyectiles
            if (proyectiles.Count != 0)
            {
                List<Tuple<Image, Proyectil>> toDelete = new List<Tuple<Image, Proyectil>>();
                foreach (Tuple<Image, Proyectil> p in proyectiles)
                {
                    p.Item2.CanvasPosX += p.Item2.SpeedX * 0.001;
                    p.Item2.CanvasPosY += p.Item2.SpeedY * 0.001;
                    p.Item2.SpeedY += gravedad * 0.001;
                    Canvas.SetLeft(p.Item1, p.Item2.CanvasPosX);
                    Canvas.SetTop(p.Item1, p.Item2.CanvasPosY);

                    //Control de Angulo
                    var controller = ImageBehavior.GetAnimationController(p.Item1);
                    var angle = Math.Atan((p.Item2.SpeedY) / (p.Item2.SpeedX)) + (Math.PI / 2);
                    var frame = 0.0;
                    if (p.Item2.SpeedX > 0)
                    {
                        //desde frame 0 hasta 16
                        frame = (angle * 16) / (Math.PI);
                    }
                    else
                    {
                        //desde frame 16 hasta 31
                        frame = (angle * 16) / (Math.PI) + 16;
                    }
                    controller.GotoFrame((int)frame);
                    //lb.Content = "angulo" + angle;


                    var grillax = (int)((p.Item2.CanvasPosX+15) / 30.0);
                    var grillay = (int)((p.Item2.CanvasPosY+15) / 30.0);
                    var elemento = "";
                    try
                    {
                        elemento = core.m.grilla[grillax, grillay];
                    }
                    catch 
                    {
                        elemento = "0";
                    }
                    if (elemento != "0") //Cuando explota
                    {
                        Explotar(p.Item2.CanvasPosX, p.Item2.CanvasPosY, p.Item2.radio, p.Item2.dano);
                        MyCanvas.Children.Remove(p.Item1);
                        toDelete.Add(p);
                        CambiarDeTurno();
                    }

                    if (p.Item2.CanvasPosY > 600) // Cuando sale de la pantalla
                    {
                        MyCanvas.Children.Remove(p.Item1);
                        toDelete.Add(p);
                        CambiarDeTurno();
                        break;
                    }
                }
                //Deleting from proyectiles
                foreach (Tuple<Image, Proyectil> p in toDelete)
                {
                    proyectiles.Remove(p);
                }
            }
            #endregion
            #region Control de Worms
            List<UCWorm> toDelete2 = new List<UCWorm>(); 
            foreach (UCWorm w in worms)
            {
                #region ControlDeSalto
                if (w.yspeed != 0 || w.onAir || true)
                {
                    w.onAir = true;
                    if (Keyboard.IsKeyDown(Key.Left) && w.Equals(turnos.Peek()))
                        MoverseIzquierda(w);
                    if (Keyboard.IsKeyDown(Key.Right) && w.Equals(turnos.Peek()))
                        MoverseDerecha(w);
                    try
                    {
                        if (core.m.grilla[(int)((w.CanvasXPos + 15.0) / 30.0), (int)((w.CanvasYPos + 68) / 30.0)] != "1" || w.yspeed < 0)
                        {
                            if (w.yspeed < 0 && core.m.grilla[(int)((w.CanvasXPos + 15.0) / 30.0), (int)((w.CanvasYPos + 30) / 30.0)] == "1")
                                w.yspeed = 0;
                            w.CanvasYPos += w.yspeed * 0.001;
                            Canvas.SetTop(w, w.CanvasYPos);
                            if ((w.xspeed > 0 && (core.m.grilla[(int)(w.CanvasXPos + 31) / 30, (int)(w.CanvasYPos + 30) / 30] != "1")) || (w.xspeed < 0 && (core.m.grilla[(int)(w.CanvasXPos - 1) / 30, (int)(w.CanvasYPos + 30) / 30] != "1")))
                            {
                                w.CanvasXPos += w.xspeed * 0.001;
                                Canvas.SetLeft(w, w.CanvasXPos);
                            }
                            w.yspeed += gravedad * 0.001;
                        }
                        else
                        {
                            w.onAir = false;
                            w.yspeed = 0;
                            w.xspeed = 0;
                        }
                    }
                    catch
                    {
                        if (w == turnos.Peek())
                            CambiarDeTurno();
                        MyCanvas.Children.Remove(w);
                        turnos = ExtensionMethods.RemoveFromQueue(w, turnos);
                        toDelete2.Add(w);
                    }
                }
                #endregion
                #region ControlDeVida
                if (w.w.hp <= 0)
                {
                    Explotar(w.CanvasXPos, w.CanvasYPos, 60, 10);
                    MyCanvas.Children.Remove(w);
                    turnos = ExtensionMethods.RemoveFromQueue(w, turnos);
                    toDelete2.Add(w);
                }else{
                    w.hp.Content = w.w.hp;
                }
                
                #endregion
            }
            //Deleting worms:
            foreach (UCWorm w in toDelete2)
            {
                worms.Remove(w);
            }
            #endregion
            #region Control de Victoria
            bool victoriaRojo = true;
            bool victoriaAzul = true;
            foreach (UCWorm w in worms)
            {
                if (w.w.equipo == Bandos.bando.azul)
                    victoriaRojo = false;
                else victoriaAzul = false;
            }
            if (victoriaRojo || victoriaAzul)
            {
                Victoria();
            }


            #endregion
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (!turnos.Peek().onAir)
                    turnos.Peek().yspeed = velocidadDeSalto;
            }
            if (e.Key == Key.Q) // para cambiar turno
            {
                turnos.Enqueue(turnos.Dequeue());
            }
            if (e.Key == Key.Left)
            {
                MoverseIzquierda(turnos.Peek());
            }
            if (e.Key == Key.Right)
            {
                MoverseDerecha(turnos.Peek());
            }
        }

        public void Explotar(double xpos, double ypos, double radio, double daño)
        {
            Image explosion = new Image();

            MyCanvas.Children.Add(explosion);
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\explotion.gif");
            image.EndInit();

            ImageBehavior.SetAnimatedSource(explosion, image);
            Canvas.SetLeft(explosion, xpos - 50);
            Canvas.SetTop(explosion, ypos);
            ImageBehavior.SetRepeatBehavior(explosion, new System.Windows.Media.Animation.RepeatBehavior(TimeSpan.FromMilliseconds(530)));
            //ImageBehavior.AnimationCompletedEvent.
            

            foreach(UCWorm w in worms)
            {
                if (Math.Sqrt((w.CanvasXPos - xpos) * (w.CanvasXPos - xpos) + (w.CanvasYPos - ypos) * (w.CanvasYPos - ypos)) < radio)
                {
                    w.w.hp -= (int)daño;
                }
            }

            //Destruccion de ladrillos
            for (int x = 0; x < core.m.largo; x++)
            {
                for (int y = 0; y < core.m.alto; y++)
                {
                    if (Math.Pow(x * 30 - xpos, 2) + Math.Pow(y * 30 - ypos, 2) < radio * radio)
                    {
                        core.m.grilla[x, y] = "0";
                        MyCanvas.Children.Remove(bloques[x, y]);
                    }
                }
            }
        }

        public void MoverseIzquierda(UCWorm worm)
        {
            var canvasposleft = (double)worm.GetValue(Canvas.LeftProperty);
            var canvaspostop = (double)worm.GetValue(Canvas.TopProperty) + 30.0;
            try
            {
                if (core.m.grilla[(int)((canvasposleft - 2) / 30), (int)Math.Ceiling(canvaspostop / 30)] != "1")
                {
                    var velocidad = 1000;
                    worm.CanvasXPos = (double)worm.GetValue(Canvas.LeftProperty) - velocidad * 0.002;
                    Canvas.SetLeft(worm, worm.CanvasXPos);
                }
            }
            catch { }
            System.Threading.Thread.Sleep(1);
            worm.onAir = true;
        }

        public void MoverseDerecha(UCWorm worm)
        {

            var canvasposleft = (double)worm.GetValue(Canvas.LeftProperty);
            var canvaspostop = (double)worm.GetValue(Canvas.TopProperty) + 30.0;
            try
            {
                if (core.m.grilla[(int)((canvasposleft + 30) / 30), (int)Math.Ceiling(canvaspostop / 30)] != "1")
                {
                    var velocidad = 1000;
                    worm.CanvasXPos = (double)worm.GetValue(Canvas.LeftProperty) + velocidad * 0.002;
                    Canvas.SetLeft(worm, worm.CanvasXPos);
                }
            }
            catch
            {

            }
            System.Threading.Thread.Sleep(1);
            worm.onAir = true;
        }

        public void CambiarDeTurno()
        {
            //Devolver el aspecto al worm que pasa a inactividad
            var worm = turnos.Dequeue();
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\9.gif");
            image.EndInit();
            ImageBehavior.SetAnimatedSource(worm.icono, image);
            turnos.Enqueue(worm);

            //Hacer que salte el worm activo
            worm = turnos.Peek();
            image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\19.gif");
            image.EndInit();
            ImageBehavior.SetAnimatedSource(worm.icono, image);
        }

        public void Victoria()
        {
            foreach (UCWorm worm in worms)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\gifs\\winner.gif");
                image.EndInit();
                ImageBehavior.SetAnimatedSource(worm.icono, image);
                GatilloActualizador.Stop();
                this.KeyDown -= MainWindow_KeyDown;
                bazookabtn.Click -= SeleccionarBazooka;
                bat.Click -= SeleccionarBat;

            }
        }



    }
}
