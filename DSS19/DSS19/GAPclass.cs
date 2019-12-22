using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PyGAP2019
{
   class GAPclass
   {  public int n;  // numero clienti
      public int m;  // numero magazzini
      public double[,] c;  // costi assegnamento
      public int[] req;    // richieste clienti
      public int[] cap;    // capacità magazzini

      public int[] sol,solbest;    // per ogni cliente, il suo magazzino
      public double zub,zlb;

      const double EPS = 0.0001;
      System.Random rnd = new Random(550);

      public GAPclass()
      {  zub = double.MaxValue;
         zlb = double.MinValue;
      }

      // costruzione, ognuno al suo deposito più vicino
      public double simpleContruct()
      {  int i,ii,j;
         int[] capleft = new int[cap.Length];
         int[] ind = new int[m];
         double[] dist = new double[m];
         Array.Copy(cap,capleft,cap.Length);

         zub = 0;
         for(j=0;j<n;j++)
         {  for(i=0;i<m;i++)
            {  dist[i]= c[i,j];
               ind[i] = i;
            }
            Array.Sort(dist,ind);
            ii=0;
            while(ii<m)
            {  i=ind[ii];
               if(capleft[i]>=req[j])
               {  sol[j]=i;
                  capleft[i] -= req[j];
                  zub += c[i,j];
                  break;
               }
               ii++;
            }
            if(ii==m)
               Trace.WriteLine("[SimpleConstruct] Ahi ahi. ii="+ii);
         }
         return zub;
      }

      // tolgo un cliente da un magazzino e lo metto in un altro
      public double opt10(double[,] c)
      {  double z=0;
         int i, isol, j;
         int[] capres = new int[cap.Length];

         Array.Copy(cap, capres, cap.Length);
         for (j = 0; j < n; j++)
         {  capres[sol[j]] -= req[j];
            z += c[sol[j],j];
         }

         l0:      for (j = 0; j < n; j++)
         {
            isol = sol[j];
            for (i = 0; i < m; i++)
            {
               if (i == isol) continue;
               if (c[i, j] < c[isol, j] && capres[i] >= req[j])
               {
                  sol[j] = i;
                  capres[i] -= req[j];
                  capres[isol] += req[j];
                  z -= (c[isol, j] - c[i, j]);
                  if(z<zub)
                  {  zub = z;
                     Trace.WriteLine("[1-0 opt] new zub " + zub);
                  }
                  goto l0;
               }
            }
         }
         double zcheck = 0;
         for (j = 0; j < n; j++)
            zcheck += c[sol[j], j];
         if (Math.Abs(zcheck - z) > EPS)
            System.Windows.Forms.MessageBox.Show("[1.0opt] Ahi ahi");
         return z;
      }

      // Tabu search
      public double TabuSearch(int Ttenure, int maxiter)
      {  int i,isol,j,imax,jmax,iter;
         double z,DeltaMax;
         int[] capres = new int[cap.Length];
         int[,] TL=new Int32[m,n];
            /*
            1.	Generate an initial feasible solution S, 
	            set S* = S and initialize TL=nil.
            2.	Find S' \in N(S), such that 
	            z(S')=min {z(S^), foreach S^\in N(S), S^\notin TL}.
            3.	S=S', TL=TL \in {S}, if (z(S*) > z(S)) set S* = S.
            4.	If not(end condition) go to step 2.
            */

         Array.Copy(cap,capres,cap.Length);
         for(j=0;j<n;j++)
            capres[sol[j]] -= req[j];

         z = zub;
         iter=0;
         for(i=0;i<m;i++)
            for(j=0;j<n;j++)
               TL[i,j]=int.MinValue;

         Trace.WriteLine("Starting tabu search");
         start: DeltaMax=imax=jmax=int.MinValue;
         iter++;

         for(j=0;j<n;j++)
         {  isol = sol[j];
            for(i=0;i<m;i++)
            {  if(i==isol) continue;
               if( (c[isol,j] - c[i,j]) > DeltaMax && capres[i] >= req[j]  && (TL[i,j]+Ttenure)<iter)
               {  imax = i;
                  jmax = j;
                  DeltaMax = c[isol,j] - c[i,j];
               }
            }
         }

         isol = sol[jmax];
         sol[jmax] = imax;
         capres[imax] -= req[jmax];
         capres[isol] += req[jmax];
         z -= DeltaMax;
         if(z < zub)
            zub = z;
         TL[imax,jmax]=iter;
         Trace.WriteLine("[Tabu Search]  z "+z+" ite "+iter+" deltamax "+DeltaMax);

         if(iter<maxiter)          // end condition
            goto start;
         else
            Trace.WriteLine("Tabu search: fine");

         double zcheck = 0;
         for(j=0;j<n;j++)
            zcheck += c[sol[j],j];
         if(Math.Abs(zcheck - z) > EPS)
            System.Windows.Forms.MessageBox.Show("[tabu search] Ahi ahi");
         return zub;
      }

      // ILS, su 1-0
      public double IteratedLocalSearch(int maxIter)
      {  int iter;
         double z;

         iter = 0;
         while(iter<maxIter)
         {  z = opt10(c);
            dataPerturbation();
            iter++;
         }
         return zub;
      }

      private void dataPerturbation()
      {  int i,j;
         double[,] cPert = new double[m,n];

         for(i=0;i<m;i++)
            for(j=0;j<n;j++)
               cPert[i,j] = c[i,j]+c[i,j]*0.5*rnd.NextDouble();

         opt10(cPert);
      }

      // controllo ammissibilità soluzione
      public double checkSol(int[] sol)
      {  double cost=0;
         int j;
         int[] capused = new int[m];

         // controllo assegnamenti
         for(j=0;j<n;j++)
            if(sol[j]<0 || sol[j]>=m)
            {  cost = double.MaxValue;
               goto lend;
            }
            else
               cost += c[sol[j],j];

         // controllo capacità
         for(j=0;j<n;j++)
         {  capused[sol[j]] += req[j];
            if(capused[sol[j]] > cap[sol[j]])
            {  cost = double.MaxValue;
               goto lend;
            }
         }

         lend:    return cost;
      }
   }
}
