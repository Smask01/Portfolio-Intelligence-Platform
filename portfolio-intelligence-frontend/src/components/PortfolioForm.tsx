import { useState } from 'react'

interface PortfolioPosition {
    ticker: string
    amountInvested: number
}

function PortfolioForm() {
    const [ticker, setTicker] = useState('')
    const [amountInvested, setAmountInvested] = useState('')
    const [positions, setPositions] = useState<PortfolioPosition[]>([])

    const addPosition = () => {
        if (!ticker || !amountInvested) return

        const newPosition: PortfolioPosition = {
            ticker: ticker.toUpperCase(),
            amountInvested: Number(amountInvested)
        }

        setPositions([...positions, newPosition])

        setTicker('')
        setAmountInvested('')
    }

    return (
        <div>
            <h2>Add Portfolio Position</h2>

            <input
                type="text"
                placeholder="Ticker"
                value={ticker}
                onChange={(event) => setTicker(event.target.value)}
            />

            <input
                type="number"
                placeholder="Amount Invested"
                value={amountInvested}
                onChange={(event) => setAmountInvested(event.target.value)}
            />

            <button onClick={addPosition}>Add Position</button>

            <h3>Portfolio</h3>

            {positions.map((position, index) => (
                <div key={index}>
                    {position.ticker} - ${position.amountInvested}
                </div>
            ))}
        </div>
    )
}

export default PortfolioForm