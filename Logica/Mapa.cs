using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Logica
{
    public class Mapa
    {
        public string[,] grilla;
        public int alto;
        public int largo;

        public Mapa(string nombre)
        {
            #region Cargar mapa desde archivo
            using (StreamReader r = new StreamReader("..\\..\\..\\"+nombre+".txt"))
            {
                string[] dimensiones = r.ReadLine().Split(new string[] { "x" }, StringSplitOptions.None);
                alto = Int32.Parse(dimensiones[0]);
                largo = Int32.Parse(dimensiones[1]);
                grilla = new string[largo, alto];

                string line;
                int x = 0;
                int y = 0;
                while ((line = r.ReadLine()) != null)
                {
                    foreach (char c in line)
                    {
                        if (c.ToString() == "1")
                        {
                            grilla[x, y] = "1";
                        }
                        else
                        {
                            grilla[x, y] = "0";
                        }
                        x++;
                    }
                    x = 0;
                    y++;
                }
            }
            #endregion
        }
        public Mapa()
        {
            #region Crear Mapa aleatorio
            alto = 15;
            largo = 50;
            grilla = new string[largo, alto];
            Random randy = new Random();
            for (int x = 0; x < largo; x++)
            {
                for (int y = alto-1; y > randy.Next(5,alto-1); y--)
                {
                    grilla[x, y] = "1";
                }
            }
            for (int x = 0; x < largo; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    if (grilla[x, y] == null)
                        grilla[x, y] = "0";
                }
            }


            #endregion
        }
        // "1" = bloque, "2" = worm, "0" = nada

        public List<Coordenada> SuperficiesPisables()
        {
            List<Coordenada> result = new List<Coordenada>();
            for (int x = 0; x < largo; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    if (grilla[x, y] == "1" && grilla[x, y - 1] == "0" && (x == 0 || grilla[x - 1, y - 1] != "2") && (x + 1 == largo || grilla[x + 1, y - 1] != "2"))
                    {
                        result.Add(new Coordenada(x, y-1));
                    }
                }
            } return result;
        }

        public void Add(Coordenada coord, string elem)
        {
            if (grilla[coord.x, coord.y] == "0")
            {
                grilla[coord.x, coord.y] = elem;
            }
            
        }

        public Coordenada GetSuelo(int x)
        {
            for (int y = 0; y < alto; y++)
            {
                if (grilla[x, y] == "1")
                {
                    return new Coordenada(x, y);
                }
            }
            return new Coordenada(x, alto-1);
        }
            
    }
}
