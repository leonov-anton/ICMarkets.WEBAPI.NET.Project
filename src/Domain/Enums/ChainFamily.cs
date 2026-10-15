namespace ICMarkets.Domain.Enums;

/// <summary>
/// Kind of network; defines which fee data a chain reports.
/// Values are persisted (TPH discriminator), so never renumber them.
/// </summary>
public enum ChainFamily
{
    /// <summary>BTC-like chains: fees per kilobyte.</summary>
    Utxo = 1,

    /// <summary>Ethereum-like chains: gas price, priority fee, base fee.</summary>
    Evm = 2,
}
