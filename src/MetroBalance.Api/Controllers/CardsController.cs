using MetroBalance.Core.Contracts;
using MetroBalance.Core.Exceptions;
using MetroBalance.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MetroBalance.Api.Controllers;

/// <summary>
/// Card enquiry endpoints. All data served here is simulated DEMO DATA.
/// </summary>
[ApiController]
[Route("api/cards")]
[Produces("application/json")]
public sealed class CardsController(ICardService cardService) : ControllerBase
{
    /// <summary>Returns the current balance for a Metro Smart Card.</summary>
    /// <param name="cardNumber">The Smart Card number (for example DMRC-DEMO-1001).</param>
    [HttpGet("{cardNumber}/balance")]
    [ProducesResponseType(typeof(ApiResponse<CardBalanceResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CardBalanceResult>>> GetBalance(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.GetBalanceAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<CardBalanceResult>.Ok(result, "Balance retrieved successfully (DEMO DATA)."));
    }

    /// <summary>Returns the recharge (top-up) history for a Metro Smart Card.</summary>
    [HttpGet("{cardNumber}/recharges")]
    [ProducesResponseType(typeof(ApiResponse<RechargeHistoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RechargeHistoryResult>>> GetRecharges(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.GetRechargeHistoryAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<RechargeHistoryResult>.Ok(result, "Recharge history retrieved successfully (DEMO DATA)."));
    }

    /// <summary>Returns the journey transaction history for a Metro Smart Card.</summary>
    [HttpGet("{cardNumber}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<TransactionHistoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionHistoryResult>>> GetTransactions(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.GetTransactionHistoryAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<TransactionHistoryResult>.Ok(result, "Transaction history retrieved successfully (DEMO DATA)."));
    }

    /// <summary>Returns balance, recharge history and journey history in a single call.</summary>
    [HttpGet("{cardNumber}/statement")]
    [ProducesResponseType(typeof(ApiResponse<CardStatementResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CardStatementResult>>> GetStatement(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.GetStatementAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<CardStatementResult>.Ok(result, "Statement retrieved successfully (DEMO DATA)."));
    }

    /// <summary>
    /// Enquiries about any card number. Cards known to MetroBalance return the full
    /// statement (DEMO DATA). Every other card returns the detected card kind and the
    /// official DMRC channels for reading a real balance - no balance is ever invented.
    /// </summary>
    /// <param name="cardNumber">Any DMRC card number, Engraved ID or demo card number.</param>
    [HttpGet("{cardNumber}/enquiry")]
    [ProducesResponseType(typeof(ApiResponse<CardEnquiryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CardEnquiryResult>>> Enquire(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.EnquireAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<CardEnquiryResult>.Ok(result, result.Message));
    }

    /// <summary>
    /// Returns only the balance amount for a card - the smallest possible answer.
    /// Cards MetroBalance knows return the simulated amount (DEMO DATA). Every other
    /// card returns <c>balanceAvailableOnline: false</c> with the DMRC policy note;
    /// no amount is ever invented.
    /// </summary>
    /// <param name="cardNumber">Any DMRC card number, Engraved ID or demo card number.</param>
    [HttpGet("{cardNumber}/amount")]
    [ProducesResponseType(typeof(ApiResponse<CardAmountResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CardAmountResult>>> GetAmount(string cardNumber, CancellationToken cancellationToken)
    {
        var result = await cardService.GetAmountAsync(cardNumber, cancellationToken);
        return Ok(ApiResponse<CardAmountResult>.Ok(result, result.Message));
    }

    /// <summary>Lists the demo card numbers that can be used to try the application.</summary>
    [HttpGet("demo-numbers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetDemoCardNumbers(CancellationToken cancellationToken)
    {
        var numbers = await cardService.GetDemoCardNumbersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<string>>.Ok(numbers, "Demo card numbers (DEMO DATA)."));
    }
}
