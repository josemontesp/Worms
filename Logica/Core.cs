using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Logica
{
    public class Core : Bandos
    {
        List<string> nombres = new List<string>();
        public Worm[] worms = new Worm[10];
        public Mapa m = new Mapa();
        public Random randy = new Random();

        public Core()
        {
            Random randy = new Random();
            #region abrir nombres.txt
            using (StreamReader r = new StreamReader("..\\..\\..\\nombres.txt"))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    nombres.Add(line);
                }
            }
            #endregion
            #region crear worms
            for (int i = 0; i < 10; i++)
            {
                bando equipo = bando.azul;
                if (i > 4)
                {
                    equipo = bando.rojo;
                }
                var index = randy.Next(0, nombres.Count() - 1);
                var nombre = nombres[index];
                nombres.RemoveAt(index);

                worms[i] = new Worm(randy, nombre, equipo, m.SuperficiesPisables());
                m.Add(worms[i].posicion, "2");
                
                
            }
            #endregion
        }

       
        
    } 
}
