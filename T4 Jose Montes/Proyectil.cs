using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4_Jose_Montes
{
    public class Proyectil
    {
        public double SpeedX = 0;
        public double SpeedY = 0;
        public double CanvasPosX = 0;
        public double CanvasPosY = 0;
        public double radio;
        public double dano;

        public Proyectil(double speedx, double speedy, double canvX, double canvY, double _radio, double _dano)
        {
            var CoeficienteDeVelocidad = 50.0; // Qué tan rapido sale a medida que alejo más el mouse
            var maximo = 200;
            if (Math.Sqrt(speedx * speedx + speedy * speedy) > maximo) // Reducimos el vector velocidad si su norma supera el maximo.
            {
                speedx = (speedx / Math.Sqrt(speedx * speedx + speedy * speedy)) * maximo;
                speedy = (speedy / Math.Sqrt(speedx * speedx + speedy * speedy)) * maximo;
            }
            SpeedX = speedx*CoeficienteDeVelocidad*1;
            SpeedY = speedy * CoeficienteDeVelocidad * 1;
            CanvasPosX = canvX;
            CanvasPosY = canvY;
            radio = _radio;
            dano = _dano;

        }
    }
}
