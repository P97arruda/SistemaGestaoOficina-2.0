
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoOficina.Data.Entities;
using SistemaGestaoOficina.Helpers;
using SistemaGestaoOficina.Models;
using System.Net;

namespace SistemaGestaoOficina.Controllers
{
    [Authorize(Roles = "Admin,Rececionista,Mecanico")]
    public class ComunicacoesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public ComunicacoesController(
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        // GET: Comunicacoes/Index

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CarregarDestinatariosAsync();

            return View(new EnviarComunicadoViewModel());
        }

        // POST: Comunicacoes/EnviarGeral

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarGeral(
            EnviarComunicadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CarregarDestinatariosAsync();
                return View("Index", model);
            }

            // VERIFICAR DESTINATÁRIOS SELECIONADOS

            var idsSelecionados = model.DestinatariosIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            if (idsSelecionados.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Selecione pelo menos um destinatário.");

                await CarregarDestinatariosAsync();
                return View("Index", model);
            }

            // OBTER DESTINATÁRIOS AUTORIZADOS

            var autorizados =
                await ObterDestinatariosPermitidosAsync();

            var idsAutorizados = autorizados
                .Select(u => u.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (idsSelecionados.Any(
                    id => !idsAutorizados.Contains(id)))
            {
                ModelState.AddModelError(
                    "",
                    "A seleção contém destinatários não autorizados.");

                await CarregarDestinatariosAsync();
                return View("Index", model);
            }

            // OBTER APENAS OS EMAILS SELECIONADOS

            var emails = autorizados
                .Where(u => idsSelecionados.Contains(u.Id))
                .Select(u => u.Email?.Trim())
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (emails.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Nenhum destinatário possui email válido.");

                await CarregarDestinatariosAsync();
                return View("Index", model);
            }

            // PREPARAR ASSUNTO E MENSAGEM

            var assunto = model.Assunto.Trim()
                .Replace("\r", " ")
                .Replace("\n", " ");

            var mensagem = model.Mensagem.Trim();

            // CRIAR O CORPO DO EMAIL

            var corpoEmail = CriarCorpoEmail(mensagem);

            // ENVIAR EMAIL INDIVIDUALMENTE

            int enviados = 0;
            int falhados = 0;

            foreach (var email in emails)
            {
                try
                {
                    var resultado = _mailHelper.SendEmail(
                        email,
                        $"AutoCET - {assunto}",
                        corpoEmail);

                    if (resultado.IsSuccess)
                    {
                        enviados++;
                    }
                    else
                    {
                        falhados++;
                    }
                }
                catch
                {
                    falhados++;
                }
            }

            // RESULTADO DO ENVIO

            if (falhados == 0)
            {
                TempData["SuccessMessage"] =
                    $"Email enviado para {enviados} destinatário(s).";
            }
            else if (enviados > 0)
            {
                TempData["WarningMessage"] =
                    $"Enviados: {enviados}. Falhados: {falhados}.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Não foi possível enviar os emails.";
            }

            return RedirectToAction(nameof(Index));
        }

        // CRIAR EMAIL SIMPLES E ALINHADO À ESQUERDA

        private string CriarCorpoEmail(string mensagem)
        {
            // PROTEGER TEXTO CONTRA HTML

            var mensagemSegura = WebUtility
                .HtmlEncode(mensagem)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "<br />");

            // EMAIL COM LAYOUT EMPRESARIAL
            // SEM CENTRALIZAÇÃO

            var html = $@"
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

                <!-- CONTEÚDO ALINHADO À ESQUERDA -->

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

                    <!-- IDENTIFICAÇÃO DA EMPRESA -->

                    <tr>
                        <td align='left'
                            style='
                                padding:6px 0 18px;
                                text-align:left;
                                border-bottom:1px solid #e5e7eb;'>

                            <span style='
                                font-size:21px;
                                font-weight:700;
                                color:#172033;'>

                                Auto<span style='color:#1683ff;'>CET</span>

                            </span>

                        </td>
                    </tr>

                    <!-- CORPO DA MENSAGEM -->

                    <tr>
                        <td align='left'
                            style='
                                padding:26px 0 12px;
                                text-align:left;
                                color:#344054;
                                font-size:15px;
                                line-height:1.75;'>

                            <!-- SAUDAÇÃO -->

                            <p style='
                                margin:0 0 20px;
                                color:#344054;
                                font-size:15px;
                                line-height:1.75;'>

                                Olá,

                            </p>

                            <!-- MENSAGEM ESCRITA PELO UTILIZADOR -->

                            <div style='
                                margin:0;
                                color:#344054;
                                font-size:15px;
                                line-height:1.75;
                                overflow-wrap:break-word;
                                text-align:left;'>

                                {mensagemSegura}

                            </div>

                            <!-- ASSINATURA -->

                            <p style='
                                margin:28px 0 5px;
                                font-size:15px;
                                line-height:1.75;
                                color:#344054;'>

                                Com os melhores cumprimentos,

                            </p>

                            <p style='
                                margin:0;
                                font-size:15px;
                                font-weight:700;
                                line-height:1.75;
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

            return html;
        }

        // DESTINATÁRIOS PERMITIDOS POR PERFIL

        private async Task<List<User>>
            ObterDestinatariosPermitidosAsync()
        {
            // ADMINISTRADOR ENVIA PARA FUNCIONÁRIOS

            if (User.IsInRole("Admin"))
            {
                var mecanicos = await _userHelper
                    .GetUsersInRoleAsync("Mecanico");

                var rececionistas = await _userHelper
                    .GetUsersInRoleAsync("Rececionista");

                return mecanicos
                    .Concat(rececionistas)
                    .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();
            }

            // RECECIONISTA E MECÂNICO ENVIAM PARA CLIENTES

            var clientes = await _userHelper
                .GetUsersInRoleAsync("Cliente");

            return clientes
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();
        }

        // CARREGAR DESTINATÁRIOS PARA A VIEW

        private async Task CarregarDestinatariosAsync()
        {
            if (User.IsInRole("Admin"))
            {
                var mecanicos = await _userHelper
                    .GetUsersInRoleAsync("Mecanico");

                var rececionistas = await _userHelper
                    .GetUsersInRoleAsync("Rececionista");

                ViewBag.MecanicosIds = mecanicos
                    .Select(u => u.Id)
                    .ToList();

                ViewBag.RececionistasIds = rececionistas
                    .Select(u => u.Id)
                    .ToList();

                ViewBag.Destinatarios = mecanicos
                    .Concat(rececionistas)
                    .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();
            }
            else
            {
                var clientes = await _userHelper
                    .GetUsersInRoleAsync("Cliente");

                ViewBag.MecanicosIds = new List<string>();
                ViewBag.RececionistasIds = new List<string>();

                ViewBag.Destinatarios = clientes
                    .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();
            }
        }
    }
}
