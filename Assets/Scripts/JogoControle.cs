using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using UnityEngine.SceneManagement;

public class JogoControle : MonoBehaviourPunCallbacks
{
    public int vezTurno; //0 = x e 1 = o
    public int contadorTurno; // conta o número de turnos jogados
    public GameObject[] iconesTurno; // mostra de quem é o turno
    public Sprite[] iconesJogador; // 0 = x icone e 1 = o icone
    public Button[] espacoTabuleiro; // espaços para jogar
    public int[] espacoMarcado; // qual espaço foi marcado por qual jogador
    [SerializeField] public TextMeshProUGUI vencedorTexto; // componente do texto vencedor
    public GameObject[] vencedorLinhas;// todas as linhas vencedoras para mostrar
    public GameObject vencedorPainel;
    public GameObject empateImagem;
    public Position pos;
    public bool block;
    private const byte Trocar_Jogador_Evento = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameComeco();
    }
    void GameComeco()
    {
        vezTurno = 0;
        block = false;
        contadorTurno = 0;
        iconesTurno[0].SetActive(true);
        iconesTurno[1].SetActive(false);
        for (int i = 0; i < espacoTabuleiro.Length; i++)
        {
            espacoTabuleiro[i].interactable = true;
            espacoTabuleiro[i].GetComponent<Image>().sprite = null;
        }
        for (int i = 0; i < espacoMarcado.Length; i++)
        {
            espacoMarcado[i] = -100;
        }
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetMouseButtonDown(0) && PhotonNetwork.LocalPlayer.ActorNumber == vezTurno + 1) 
         {
            jogodaVelhaBotao(pos.varPos);
         }
    }

    public void setPosition(int num)
    {
        pos.varPos = num;
    }
    
    public void jogodaVelhaBotao(int qualNumero)
    {
        if (espacoMarcado[qualNumero] == -100 && contadorTurno % 2 == PhotonNetwork.LocalPlayer.ActorNumber - 1)
        {
            espacoTabuleiro[qualNumero].image.sprite = iconesJogador[vezTurno];
            espacoTabuleiro[qualNumero].interactable = false;

            espacoMarcado[qualNumero] = vezTurno + 1;
            contadorTurno++;
            if (contadorTurno > 4)
            {
                bool ehVencedor = verificarVencedor(qualNumero, vezTurno, contadorTurno);
                if (contadorTurno == 9 && !ehVencedor)
                {
                    jogoEmpate();
                }
                enviarInfoJogadaRede(qualNumero, vezTurno, contadorTurno);
            }
            if (vezTurno == 0)
            {
                enviarInfoJogadaRede(qualNumero, vezTurno, contadorTurno);
                vezTurno = 1;
                atualizaTurno(false);


            }
            else if (vezTurno != 0)
            {
                enviarInfoJogadaRede(qualNumero,vezTurno,contadorTurno);
                vezTurno = 0;
                atualizaTurno(true);

            }
            
        }
        

    }
    
    public void enviarInfoJogadaRede(int posTabuleiro,int ultimoJogador,int contSessao)
    {
        object[] datas = new object[] { posTabuleiro, ultimoJogador, contSessao };
        PhotonNetwork.RaiseEvent(Trocar_Jogador_Evento, datas, RaiseEventOptions.Default, SendOptions.SendReliable);
    }
    bool verificarVencedor(int qualNumero, int vezTurno,int contadorTurno)
    {
        int s1 = espacoMarcado[0] + espacoMarcado[1] + espacoMarcado[2];
        int s2 = espacoMarcado[3] + espacoMarcado[4] + espacoMarcado[5];
        int s3 = espacoMarcado[6] + espacoMarcado[7] + espacoMarcado[8];
        int s4 = espacoMarcado[0] + espacoMarcado[3] + espacoMarcado[6];
        int s5 = espacoMarcado[1] + espacoMarcado[4] + espacoMarcado[7];
        int s6 = espacoMarcado[2] + espacoMarcado[5] + espacoMarcado[8];
        int s7 = espacoMarcado[0] + espacoMarcado[4] + espacoMarcado[8];
        int s8 = espacoMarcado[2] + espacoMarcado[4] + espacoMarcado[6];
        var solucoes = new int[] { s1, s2, s3, s4, s5, s6, s7, s8 };
        for (int i = 0; i < solucoes.Length; i++)
        {
            if (solucoes[i] == 3 * (vezTurno + 1))
            {
                mostrarVencedor(i);
                enviarInfoJogadaRede(qualNumero, vezTurno, contadorTurno);
                Debug.Log("Jogador " + vezTurno + " venceu!");
                return true;
            }
        }
        return false;
    }

    void mostrarVencedor(int indexDentro)
    {
        vencedorPainel.gameObject.SetActive(true);
        if (vezTurno == 0)
        {
            vencedorTexto.text = "Jogador X venceu!";
        }
        else if (vezTurno == 1)
        {
            vencedorTexto.text = "Jogador O venceu!";
        }

        vencedorLinhas[indexDentro].SetActive(true);

    }
    
    void jogoEmpate()
    {
        vencedorPainel.SetActive(true);
        empateImagem.SetActive(true);
        vencedorTexto.text = "Empate";

    }

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += netReceber;
    }
    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= netReceber;
    }

    private void netReceber(EventData obj)
    {
        if (obj.Code == Trocar_Jogador_Evento)
        {
            object[] datas = (object[])obj.CustomData;
            int botaoEscolhido = (int)datas[0]; //numero escolhido
            int idUltimojogadorTurno = (int)datas[1];//ultimo turno
            int sessaoAtual = (int)datas[2];//sessao
            if (contadorTurno < sessaoAtual)
            {
                atualizaTela(botaoEscolhido, idUltimojogadorTurno,sessaoAtual);
                contadorTurno = sessaoAtual;
            }
            


        }
    }
    public void atualizaTela(int posEscolhidaTabela,int idUltimojogadorTurno, int sessaoAtual)
        {
        espacoTabuleiro[posEscolhidaTabela].image.sprite = iconesJogador[idUltimojogadorTurno];
        espacoTabuleiro[posEscolhidaTabela].interactable = false;
        espacoMarcado[posEscolhidaTabela] = idUltimojogadorTurno + 1;
        atualizaTurno((idUltimojogadorTurno + 1)%2==0);
        if (!terminar(posEscolhidaTabela, idUltimojogadorTurno, sessaoAtual))
        {
            vezTurno = Convert.ToBoolean(vezTurno) ? 0 : 1;
        }
        
        

    }

    public void atualizaTurno(bool meuTurno)
    {
        iconesTurno[0].SetActive(meuTurno);
        iconesTurno[1].SetActive(!meuTurno);
    }

    public bool terminar(int qualNumero, int vezTurno, int contadorTurno)
    {
        bool ehVencedor = verificarVencedor(qualNumero, vezTurno, contadorTurno);
        if (contadorTurno == 9 && ehVencedor == false)
        {
            jogoEmpate();
        }
        return ehVencedor;
    }

    [PunRPC]
    public void voltarMenu_rpc()
    {
        SceneManager.LoadScene(0);
    }

    public void voltarMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
        GameComeco();
        Debug.Log("Cliquei");

    }

}

