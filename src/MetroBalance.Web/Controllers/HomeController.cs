using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MetroBalance.Core.Interfaces;
using MetroBalance.Web.Models;

namespace MetroBalance.Web.Controllers;

public class HomeController(ICardService cardService) : Controller
{
    /// <summary>The sample card shown in the homepage hero card.</summary>
    private const string SampleCardNumber = "DMRC-DEMO-1001";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new HomeIndexViewModel();

        try
        {
            var enquiry = await cardService.EnquireAsync(SampleCardNumber, cancellationToken);
            var statement = enquiry.Statement;

            if (statement is not null)
            {
                model.CardHolderName = statement.Balance.CardHolderName;
                model.CardType = statement.Balance.CardType;
                model.BalanceFormatted = statement.Balance.BalanceFormatted;
                model.LastUpdatedFormatted = statement.Balance.LastUpdatedOnFormatted ?? "—";

                var lastJourney = statement.Transactions.FirstOrDefault();
                if (lastJourney is not null)
                {
                    model.LastJourney = $"{lastJourney.FromStation} → {lastJourney.ToStation}";
                }
            }
        }
        catch
        {
            // The homepage must render even if the sample card is unavailable.
        }

        return View(model);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Faq()
    {
        return View();
    }

    /// <summary>Step by step guide for reading the balance of a real DMRC Smart Card.</summary>
    public IActionResult Guide()
    {
        return View();
    }

    /// <summary>Presentation deck used to showcase the project to DMRC and other stakeholders.</summary>
    public IActionResult Showcase()
    {
        ViewData["Title"] = "Showcase";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
