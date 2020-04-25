using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alchemy.Classes;
using Newtonsoft.Json;

namespace secondo
{
    class Msg
    {
        public string testo;
        public string nomeMittente;
        public string nomeDestinatario;
        public int tipoMsg;
        public string psw;

        public Msg()
        {

        }

        public override string ToString()
        {
            return String.Format("{0} | {1} | {2} | {3}", testo, nomeMittente, nomeDestinatario, tipoMsg);
        }
    }

    class MsgClient
    {
        public int tipoMsg;
        public string nomeMittente;
        public string mittenteGruppi;
        public bool result;
        public string messaggio;
        public string admin;

        public MsgClient()
        {

        }
    }

    class Server
    {
        static public List<Gruppo> gruppi = new List<Gruppo>();
        static public List<Utente> utenti = new List<Utente>();
        static public void GestisciMessaggio(UserContext c)
        {
            Msg m = JsonConvert.DeserializeObject<Msg>(c.DataFrame.ToString());
            MsgClient mc = new MsgClient();
            switch (m.tipoMsg)
            {
                case 0: //messaggio
                    if (InviaMessaggio(m))
                    {
                        Console.WriteLine("messaggio inviato");

                    }
                    else
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "nome utente non trovato";
                        Console.WriteLine("errore");
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 1: //nuovoUtente
                    if (Registrazione(m, c))
                    {
                        mc.tipoMsg = 1;
                        mc.result = true;
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    else
                    {
                        mc.tipoMsg = 1;
                        mc.result = false;
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 2://login
                    if (Login(m, c))
                    {
                        mc.tipoMsg = 2;
                        mc.result = true;
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    else
                    {
                        mc.tipoMsg = 2;
                        mc.result = false;
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 3:
                    if (NewGroup(m))
                    {
                        m.nomeMittente = "";
                        m.nomeDestinatario = m.testo;
                        SendToGroup(m);
                    }
                    else
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "nessun utente inserito è stato trovato";
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 4:
                    if (!SendToGroup(m))
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "errore gruppo non trovato";
                        Console.WriteLine("errore");
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 5:
                    if(EliminaGruppo(m))
                    {

                    }
                    break;
                case 6:
                    if (AddToGroup(m))
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "aggiunto con successo";
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    else
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "utente non trovato o già presente nel gruppo";
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
                case 7:
                    if (EliminaSingolo(m))
                    {
                        if (m.testo != m.nomeMittente)
                        {
                            mc.tipoMsg = 3;
                            mc.messaggio = "rimosso con successo";
                            string s = JsonConvert.SerializeObject(mc);
                            c.Send(s);
                        }
                        
                    }
                    else
                    {
                        mc.tipoMsg = 3;
                        mc.messaggio = "utente non presente nel gruppo";
                        string s = JsonConvert.SerializeObject(mc);
                        c.Send(s);
                    }
                    break;
            }
        }

        static private bool Registrazione(Msg m, UserContext c)
        {
            bool result = false;
            int i = utenti.FindIndex((P) => P.nome == m.nomeMittente);
            if (i == -1)
            {
                Utente u = new Utente(m.nomeMittente, c, m.psw);
                utenti.Add(u);
                result = true;
            }
            return result;
        }

        static private bool Login(Msg m, UserContext c)
        {
            bool result = false;
            int i = utenti.FindIndex((P) => P.nome == m.nomeMittente);
            if (i != -1)
            {
                if (utenti[i].psw == m.psw)
                {
                    utenti[i].c = c;
                    result = true;
                }
            }
            return result;
        }

        static private bool InviaMessaggio(Msg m)
        {
            bool result = false;
            int i = utenti.FindIndex((P) => P.nome == m.nomeDestinatario);
            int j = utenti.FindIndex((P) => P.nome == m.nomeMittente);
            if (utenti[j].psw == m.psw)
            {
                if (i != -1)
                {
                    MsgClient mc = new MsgClient();
                    mc.messaggio = m.testo;
                    mc.tipoMsg = 0;
                    mc.nomeMittente = m.nomeMittente;
                    utenti[i].c.Send(JsonConvert.SerializeObject(mc));
                    result = true;
                }
            }
            return result;
        }

        static private bool NewGroup(Msg m)
        {
            Gruppo g = new Gruppo();
            bool result = false;

            int i;
            int j = utenti.FindIndex((P) => P.nome == m.nomeMittente);
            if (utenti[j].psw == m.psw)
            {
                int ig = gruppi.FindIndex((P) => P.nome == m.testo);
                if (ig == -1)
                {
                    g.nome = m.testo;
                    g.admin = m.nomeMittente;
                    m.nomeDestinatario += m.nomeMittente;
                    string[] dest;
                    dest = m.nomeDestinatario.Split(';');
                    foreach (var s in dest)
                    {
                        i = utenti.FindIndex((P) => P.nome == s);
                        if (i != -1)
                        {
                            result = true;
                            g.utenti.Add(utenti[i]);
                        }
                    }
                    if (result)
                    {
                        gruppi.Add(g);

                    }
                }
                else
                {
                    MsgClient mc = new MsgClient();
                    mc.tipoMsg = 3;
                    mc.messaggio = "nome gruppo già in uso";
                    string s = JsonConvert.SerializeObject(mc);
                    utenti.Find((P) => P.nome == m.nomeMittente).c.Send(s);
                    result = true;
                }
            }


            return result;
        }

        static private bool SendToGroup(Msg m)
        {
            bool result = false;
            int ig = gruppi.FindIndex((P) => P.nome == m.nomeDestinatario);
            if (ig != -1)
            {

                MsgClient mc = new MsgClient();
                mc.nomeMittente = m.nomeDestinatario;
                mc.tipoMsg = 4;
                mc.messaggio = m.testo;
                mc.mittenteGruppi = m.nomeMittente;
                gruppi[ig].send(mc);
                result = true;
            }
            return result;
        }

        static private bool EliminaGruppo(Msg m)
        {
            bool result = false;
            int ig = gruppi.FindIndex((P) => P.nome == m.nomeDestinatario);
            if (ig!=-1)
            {
                if (gruppi[ig].admin == m.nomeMittente)
                {
                    gruppi[ig].Elimina();
                    gruppi.Remove(gruppi[ig]);
                    result = true;
                }

            }
            return result;
        }

        static private bool AddToGroup(Msg m)
        {
            bool result = false;
            int iUtente = utenti.FindIndex((P) => P.nome == m.testo);
            if (iUtente != -1)
            {
                int ig = gruppi.FindIndex((P) => P.nome == m.nomeDestinatario);
                if (ig != -1)
                {
                    if (gruppi[ig].admin == m.nomeMittente)
                    {
                        int x = gruppi[ig].utenti.FindIndex((P) => P == utenti[iUtente]);
                        if (x == -1)
                        {
                            result = true;
                            gruppi[ig].utenti.Add(utenti[iUtente]);
                            MsgClient msg = new MsgClient();
                            msg.tipoMsg = 4;
                            msg.admin = gruppi[ig].admin;
                            msg.nomeMittente = gruppi[ig].nome;
                            string s = JsonConvert.SerializeObject(msg);
                            utenti[iUtente].c.Send(s);
                        }                     
                    }
                }
            }
            return result;
        }

        static private bool EliminaSingolo(Msg m)
        {
            bool result = false;
            int indexGroup = gruppi.FindIndex(P => P.nome == m.nomeDestinatario);
            if (indexGroup != -1)
            {
                int indexElim = gruppi[indexGroup].utenti.FindIndex(P => P.nome == m.testo);
                if (indexElim != -1)
                {
                    result = true;
                    gruppi[indexGroup].EliminaUtente(indexElim);
                }
            }
            return result;
        }
    }

    class Utente
    {
        public string nome;
        public string psw;
        public UserContext c;

        public Utente(string nome, UserContext c, string psw)
        {
            this.nome = nome;
            this.c = c;
            this.psw = psw;
        }
    }

    class Gruppo
    {
        public List<Utente> utenti;
        public string nome;
        public string admin;

        public Gruppo()
        {
            utenti = new List<Utente>();
        }

        public bool send(MsgClient mc)
        {
            bool result = false;
            mc.admin = admin;
            mc.messaggio = "<span>" + mc.mittenteGruppi + "</span><br>" + mc.messaggio;
            string s = JsonConvert.SerializeObject(mc);
            foreach (var u in utenti)
            {
                try
                {
                    if (u.nome != mc.mittenteGruppi)
                    {
                        u.c.Send(s);
                        result = true;
                    }
                    
                }
                catch
                {

                }
            }
            return result;
        }

        public void Elimina()
        {
            MsgClient mc = new MsgClient();
            mc.tipoMsg = 5;
            mc.messaggio = nome;
            string s = JsonConvert.SerializeObject(mc);
            foreach (var u in utenti)
            {
                u.c.Send(s);
            }
        }

        public void EliminaUtente(int index)
        {
            MsgClient mc = new MsgClient();
            mc.tipoMsg = 5;
            mc.messaggio = nome;
            string s = JsonConvert.SerializeObject(mc);
            utenti[index].c.Send(s);
            utenti.RemoveAt(index);
        }
    }
}
