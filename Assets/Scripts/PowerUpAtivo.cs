[System.Serializable]
public class PowerUpAtivo
{
    public int codigo;
    public float tempoRestante;

    public PowerUpAtivo(int codigo, float duracao)
    {
        this.codigo = codigo;
        this.tempoRestante = duracao;
    }
}