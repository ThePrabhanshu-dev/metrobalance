using MetroBalance.Core.Contracts;
using MetroBalance.Core.Exceptions;
using MetroBalance.Core.Interfaces;
using MetroBalance.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MetroBalance.Web.Controllers;

/// <summary>
/// Server rendered card enquiry page. It consumes the same <see cref="ICardService"/>
/// used by the Web API, so it works without JavaScript.
/// </summary>
public sealed class CardController(ICardService cardService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? cardNumber, string? mode, CancellationToken cancellationToken)
    {
        var model = new CardLookupViewModel
        {
            CardNumber = cardNumber ?? string.Empty,
            AmountOnly = string.Equals(mode, "amount", StringComparison.OrdinalIgnoreCase),
            DemoCardNumbers = await cardService.GetDemoCardNumbersAsync(cancellationToken)
        };

        if (!string.IsNullOrWhiteSpace(cardNumber))
        {
            await LoadResultsAsync(model, cardNumber, cancellationToken);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lookup(string cardNumber, string? mode, CancellationToken cancellationToken)
    {
        var model = new CardLookupViewModel
        {
            CardNumber = cardNumber ?? string.Empty,
            AmountOnly = string.Equals(mode, "amount", StringComparison.OrdinalIgnoreCase),
            DemoCardNumbers = await cardService.GetDemoCardNumbersAsync(cancellationToken)
        };

        await LoadResultsAsync(model, cardNumber, cancellationToken);
        return View("Index", model);
    }

    private async Task LoadResultsAsync(CardLookupViewModel model, string? cardNumber, CancellationToken cancellationToken)
    {
        try
        {
            if (model.AmountOnly)
            {
                model.Amount = await cardService.GetAmountAsync(cardNumber ?? string.Empty, cancellationToken);
                return;
            }

            var enquiry = await cardService.EnquireAsync(cardNumber ?? string.Empty, cancellationToken);
            model.Enquiry = enquiry;
            model.Statement = enquiry.Statement;
        }
        catch (InvalidCardNumberException ex)
        {
            model.ErrorMessage = ex.Message;
        }
        catch (CardNotFoundException ex)
        {
            model.ErrorMessage = ex.Message + " Try one of the demo card numbers listed below.";
        }
        catch (CardNotActiveException ex)
        {
            model.ErrorMessage = ex.Message;
        }
    }
}
