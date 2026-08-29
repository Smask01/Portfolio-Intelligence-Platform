import { useState } from 'react'

interface PortfolioPosition {
    ticker: string
    amountInvested: number
}

interface HoldingExposure {
    symbol: string
    companyName: string
    amountExposed: number
    portfolioPercentage: number
}

interface SectorExposure {
    sector: string
    amountExposed: number
    portfolioPercentage: number
}

interface EtfOverlap {
    firstTicker: string
    secondTicker: string
    overlap: number
}

interface AnalyzePortfolioResponse {
    holdingExposures: HoldingExposure[]
    sectorExposures: SectorExposure[]
    overlaps: EtfOverlap[]
}

function PortfolioForm() {
    const [ticker, setTicker] = useState('')
    const [amountInvested, setAmountInvested] = useState('')
    const [positions, setPositions] = useState<PortfolioPosition[]>([])
    const [analysisResult, setAnalysisResult] = useState<AnalyzePortfolioResponse | null>(null)
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    
    const analyzePortfolio = async () => {
        setIsLoading(true)
        setError(null)
        try {
            const response = await fetch('http://localhost:5015/api/portfolio/analyze', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    positions
                })
            })

            if (!response.ok) throw new Error('Failed to analyze portfolio.')

            const data: AnalyzePortfolioResponse = await response.json()
            setAnalysisResult(data)
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Something went wrong.')
        } finally {
            setIsLoading(false)
        }
    }
    const removePosition = (indexToRemove: number) => {
        setPositions(positions.filter((_, index) => index !== indexToRemove))
        setAnalysisResult(null)
    }
    const addPosition = () => {
        if (!ticker || !amountInvested) return

        const newPosition: PortfolioPosition = {
            ticker: ticker.toUpperCase(),
            amountInvested: Number(amountInvested)
        }

        setPositions([...positions, newPosition])
        setAnalysisResult(null)

        setTicker('')
        setAmountInvested('')
    }

    return (
        <div className="portfolio-form">
            <h2>Add Portfolio Position</h2>

            <div className="input-row">
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
            </div>

            <h3>Portfolio</h3>

            {positions.map((position, index) => (
                <div className="position-row" key={index}>
                    <span> {position.ticker} - ${position.amountInvested} </span>
                    <button onClick={() => removePosition(index)}>
                        Remove
                    </button>
                </div>
            ))}

            <button onClick={analyzePortfolio} disabled={isLoading || positions.length === 0}>
                {isLoading ? 'Analyzing...' : 'Analyze Portfolio'}
            </button>

            {error && <p>{error}</p>}
            
            {analysisResult && (
                <div>
                    <h2>Holding Exposure</h2>

                    <table>
                        <thead>
                        <tr>
                            <th>Symbol</th>
                            <th>Company</th>
                            <th>Exposure</th>
                            <th>Portfolio %</th>
                        </tr>
                        </thead>

                        <tbody>
                        {analysisResult.holdingExposures.map((holding) => (
                            <tr key={holding.symbol}>
                                <td>{holding.symbol}</td>
                                <td>{holding.companyName}</td>
                                <td>${holding.amountExposed.toFixed(2)}</td>
                                <td>{holding.portfolioPercentage.toFixed(2)}%</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    <h2>Sector Exposure</h2>

                    <table>
                        <thead>
                        <tr>
                            <th>Sector</th>
                            <th>Exposure</th>
                            <th>Portfolio %</th>
                        </tr>
                        </thead>

                        <tbody>
                        {analysisResult.sectorExposures.map((sector) => (
                            <tr key={sector.sector}>
                                <td>{sector.sector}</td>
                                <td>${sector.amountExposed.toFixed(2)}</td>
                                <td>{sector.portfolioPercentage.toFixed(2)}%</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    <h2>ETF Overlap</h2>
                    {analysisResult.overlaps.map((overlap) => (
                        <div key={`${overlap.firstTicker}-${overlap.secondTicker}`}>
                            {overlap.firstTicker} / {overlap.secondTicker} - {(overlap.overlap * 100).toFixed(2)}%
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}

export default PortfolioForm