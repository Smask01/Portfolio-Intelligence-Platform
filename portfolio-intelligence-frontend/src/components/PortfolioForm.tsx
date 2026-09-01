import { useState } from 'react'
import {PieChart, Pie, Tooltip, Legend, BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer} from 'recharts'

type AssetType = 'Etf' | 'Stock'

interface PortfolioPosition {
    ticker: string
    amountInvested: number
    assetType: AssetType
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
    const [assetType, setAssetType] = useState<AssetType>('Etf')
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
            setCurrentPage(1)
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
            amountInvested: Number(amountInvested),
            assetType
        }

        setPositions([...positions, newPosition])
        setAnalysisResult(null)

        setTicker('')
        setAmountInvested('')
    }

    const totalPortfolioValue = positions.reduce(
        (total, position) => total + position.amountInvested,
        0
    )

    const topHoldings = analysisResult
        ? [...analysisResult.holdingExposures]
            .sort((a, b) => b.portfolioPercentage - a.portfolioPercentage)
            .slice(0, 10)
        : []

    const [currentPage, setCurrentPage] = useState(1)

    const rowsPerPage = 10

    const totalPages = analysisResult
        ? Math.ceil(analysisResult.holdingExposures.length / rowsPerPage)
        : 0

    const paginatedHoldings = analysisResult
        ? analysisResult.holdingExposures.slice(
            (currentPage - 1) * rowsPerPage,
            currentPage * rowsPerPage
        )
        : []
    
    return (
        <div className="portfolio-form">
            <h3>Add Portfolio Position</h3>
            <p>Total Portfolio Value: ${totalPortfolioValue.toFixed(2)}</p>
            
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
                
                <select
                    value={assetType}
                    onChange={(e) =>
                        setAssetType(e.target.value as AssetType)
                    }
                >
                    <option value="Etf">ETF</option>
                    <option value="Stock">Stock</option>
                </select>

                <button onClick={addPosition}>Add Position</button>
            </div>

            <h3>Portfolio</h3>

            {positions.map((position, index) => (
                <div className="position-row" key={index}>
                    <span>{position.ticker} ({position.assetType}) - ${position.amountInvested}</span>
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
                    <div className="summary-cards">
                        <div className="summary-card">
                            <h3>Total Value</h3>
                            <p>${totalPortfolioValue.toFixed(2)}</p>
                        </div>

                        <div className="summary-card">
                            <h3>Positions</h3>
                            <p>{positions.length}</p>
                        </div>

                        <div className="summary-card">
                            <h3>Holdings</h3>
                            <p>{analysisResult.holdingExposures.length}</p>
                        </div>
                    </div>
                    
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
                        {paginatedHoldings.map((holding) => (
                            <tr key={holding.symbol}>
                                <td>{holding.symbol}</td>
                                <td>{holding.companyName}</td>
                                <td>${holding.amountExposed.toFixed(2)}</td>
                                <td>{holding.portfolioPercentage.toFixed(2)}%</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    <div className="pagination">
                        <button
                            onClick={() => setCurrentPage(currentPage - 1)}
                            disabled={currentPage === 1}
                        >
                            Previous
                        </button>

                        <span>
                             Page {currentPage} of {totalPages}
                        </span>

                        <button
                            onClick={() => setCurrentPage(currentPage + 1)}
                            disabled={currentPage === totalPages}
                        >
                            Next
                        </button>
                    </div>

                    <h2>Top Holdings</h2>

                    <BarChart
                        width={900}
                        height={400}
                        data={topHoldings}
                    >
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="symbol" />
                        <YAxis />
                        <Tooltip
                            formatter={(value) => `${Number(value).toFixed(2)}%`}
                        />
                        <Bar
                            dataKey="portfolioPercentage"
                            name="Portfolio %"
                        />
                    </BarChart>

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

                    <div style={{ width: '100%', height: 400 }}>
                        <ResponsiveContainer>
                            <PieChart>
                                <Pie
                                    data={analysisResult.sectorExposures}
                                    dataKey="portfolioPercentage"
                                    nameKey="sector"
                                    outerRadius={130}
                                    label={({ name, value }) => `${name}: ${Number(value).toFixed(1)}%`}
                                />
                                <Tooltip
                                    formatter={(value) => `${Number(value).toFixed(2)}%`}
                                />
                                <Legend />
                            </PieChart>
                        </ResponsiveContainer>
                    </div>

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