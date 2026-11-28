using System.Net;
using SistemaGestaoOficina.Helpers;

namespace SistemaGestaoOficina.Services
{
    public class NotificacoesMarcacaoService
    {
        private readonly IMailHelper _mailHelper;

        public NotificacoesMarcacaoService(IMailHelper mailHelper)
        {
            _mailHelper = mailHelper;
        }

        // EMAIL DE CANCELAMENTO

        public bool EnviarCancelamento(
            string emailCliente,
            DateTime dataHora,
            string marca,
            string modelo,
            string matricula)
        {
            string data = dataHora.ToString("dd/MM/yyyy");
            string hora = dataHora.ToString("HH:mm");

            string veiculo = $"{marca} {modelo}".Trim();

            string mensagem = $@"
<p style='margin:0 0 20px;'>
    Informamos que a sua marcação do dia
    <strong>{WebUtility.HtmlEncode(data)}</strong>,
    às <strong>{WebUtility.HtmlEncode(hora)}</strong>,
    referente ao veículo
    <strong>{WebUtility.HtmlEncode(veiculo)}</strong>,
    com a matrícula
    <strong>{WebUtility.HtmlEncode(matricula)}</strong>,
    foi cancelada.
</p>

<p style='margin:0 0 20px;'>
    Se solicitou este cancelamento, por favor, ignore
    este email. Caso contrário, a nossa equipa entrará
    em contacto consigo nas próximas horas para
    prestar os devidos esclarecimentos.
</p>";

            return EnviarEmail(
                emailCliente,
                "AutoCET - Cancelamento da sua marcação",
                mensagem);
        }

        // EMAIL DE ALTERAÇÃO

        public bool EnviarAlteracao(
            string emailCliente,
            DateTime dataHora,
            string marca,
            string modelo,
            string matricula,
            string? tipoServico)
        {
            string data = dataHora.ToString("dd/MM/yyyy");
            string hora = dataHora.ToString("HH:mm");

            string veiculo = $"{marca} {modelo}".Trim();

            string mensagem = $@"
<p style='margin:0 0 20px;'>
    Informamos que a sua marcação foi alterada.
</p>

<p style='margin:0 0 20px;'>
    <strong>Nova data:</strong>
    {WebUtility.HtmlEncode(data)}<br />

    <strong>Nova hora:</strong>
    {WebUtility.HtmlEncode(hora)}<br />

    <strong>Veículo:</strong>
    {WebUtility.HtmlEncode(veiculo)}<br />

    <strong>Matrícula:</strong>
    {WebUtility.HtmlEncode(matricula)}<br />

    <strong>Serviço:</strong>
    {WebUtility.HtmlEncode(tipoServico ?? "Não especificado")}
</p>

<p style='margin:0 0 20px;'>
    Se não solicitou esta alteração, a nossa equipa
    entrará em contacto consigo nas próximas horas
    para prestar os devidos esclarecimentos.
</p>";

            return EnviarEmail(
                emailCliente,
                "AutoCET - Alteração da sua marcação",
                mensagem);
        }

        // ENVIO ATRAVÉS DO MAILHELPER

        private bool EnviarEmail(
            string emailCliente,
            string assunto,
            string mensagemHtml)
        {
            if (string.IsNullOrWhiteSpace(emailCliente))
            {
                return false;
            }

            try
            {
                string corpoEmail =
                    CriarCorpoEmail(mensagemHtml);

                var resultado = _mailHelper.SendEmail(
                    emailCliente.Trim(),
                    assunto,
                    corpoEmail);

                return resultado.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        // FORMATO EMPRESARIAL DA AUTOCET

        private string CriarCorpoEmail(string mensagemHtml)
        {
            return $@"
<!DOCTYPE html>
<html lang='pt'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport'
          content='width=device-width, initial-scale=1.0'>
</head>

<body style='
    margin:0;
    padding:0;
    background-color:#ffffff;
    font-family:Arial,Helvetica,sans-serif;
    color:#344054;'>

    <table role='presentation'
           width='100%'
           cellpadding='0'
           cellspacing='0'
           border='0'
           style='
                width:100%;
                background-color:#ffffff;
                border-collapse:collapse;'>

        <tr>
            <td align='left'
                style='
                    padding:16px 20px;
                    text-align:left;'>

                <table role='presentation'
                       width='100%'
                       cellpadding='0'
                       cellspacing='0'
                       border='0'
                       align='left'
                       style='
                            width:100%;
                            max-width:760px;
                            margin:0;
                            border-collapse:collapse;
                            text-align:left;'>

                    <!-- NOME DA EMPRESA -->

                    <tr>
                        <td align='left'
                            style='
                                padding:6px 0 18px;
                                border-bottom:1px solid #e5e7eb;'>

                            <span style='
                                font-size:21px;
                                font-weight:700;
                                color:#172033;'>

                                Auto<span style='color:#1683ff;'>CET</span>

                            </span>

                        </td>
                    </tr>

                    <!-- CONTEÚDO -->

                    <tr>
                        <td align='left'
                            style='
                                padding:26px 0 12px;
                                color:#344054;
                                font-size:15px;
                                line-height:1.75;
                                text-align:left;'>

                            <p style='margin:0 0 20px;'>
                                Olá,
                            </p>

                            {mensagemHtml}

                            <!-- ASSINATURA -->

                            <p style='margin:28px 0 5px;'>
                                Com os melhores cumprimentos,
                            </p>

                            <p style='
                                margin:0;
                                font-weight:700;
                                color:#172033;'>

                                Equipa AutoCET

                            </p>

                        </td>
                    </tr>

                </table>

            </td>
        </tr>

    </table>

</body>
</html>";
        }
    }
}