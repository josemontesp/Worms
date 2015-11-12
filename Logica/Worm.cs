using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class Worm : Bandos
    {
        public int hp = 100;
        public Coordenada posicion;
        public string nombre;
        public bando equipo;

        public Worm(Random r, string nombre, bando equipo, List<Coordenada> pisable)
        {
            this.nombre = nombre;
            this.equipo = equipo;
            this.posicion = pisable[r.Next(0, pisable.Count)];
            
        }

    }
}
