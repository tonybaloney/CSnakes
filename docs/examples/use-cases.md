# Real-world Use Cases

This section showcases practical applications of CSnakes in real-world scenarios.

## Data Science and Analytics

### Customer Segmentation with Machine Learning

**Scenario**: An e-commerce platform needs to segment customers for targeted marketing campaigns.

```python
# customer_segmentation.py
import pandas as pd
import numpy as np
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler
from typing import List, Dict, Tuple

def segment_customers(customer_data: list[dict[str, float]]) -> tuple[list[int], dict[str, list[float]]]:
    """
    Segment customers based on purchase behavior.
    
    Args:
        customer_data: List of customer records with 'total_spent', 'frequency', 'recency'
    
    Returns:
        Tuple of (customer_segments, segment_characteristics)
    """
    # Convert to DataFrame
    df = pd.DataFrame(customer_data)
    
    # Feature scaling
    scaler = StandardScaler()
    features_scaled = scaler.fit_transform(df[['total_spent', 'frequency', 'recency']])
    
    # K-means clustering
    kmeans = KMeans(n_clusters=4, random_state=42)
    segments = kmeans.fit_predict(features_scaled)
    
    # Calculate segment characteristics
    df['segment'] = segments
    characteristics = {}
    for segment in range(4):
        segment_data = df[df['segment'] == segment]
        characteristics[f"segment_{segment}"] = [
            float(segment_data['total_spent'].mean()),
            float(segment_data['frequency'].mean()),
            float(segment_data['recency'].mean()),
            len(segment_data)
        ]
    
    return segments.tolist(), characteristics

def recommend_marketing_strategy(segment_id: int) -> dict[str, str]:
    """Recommend marketing strategy based on customer segment."""
    strategies = {
        0: {
            "name": "High Value Customers",
            "strategy": "VIP program, exclusive offers",
            "message": "Thank you for being a loyal customer"
        },
        1: {
            "name": "Regular Customers", 
            "strategy": "Loyalty rewards, cross-selling",
            "message": "Discover more products you'll love"
        },
        2: {
            "name": "At-Risk Customers",
            "strategy": "Win-back campaigns, discounts",
            "message": "We miss you! Here's a special offer"
        },
        3: {
            "name": "New Customers",
            "strategy": "Onboarding, educational content",
            "message": "Welcome! Let us help you get started"
        }
    }
    return strategies.get(segment_id, strategies[1])
```

```csharp
// C# Usage in an ASP.NET Core application
public class CustomerSegmentationService
{
    private readonly IPythonEnvironment _python;
    
    public CustomerSegmentationService(IPythonEnvironment python)
    {
        _python = python;
    }
    
    public async Task<CustomerSegmentationResult> SegmentCustomersAsync(List<CustomerData> customers)
    {
        var segmentation = _python.CustomerSegmentation();
        
        // Convert to Python format
        var pythonData = customers.Select(c => new Dictionary<string, double>
        {
            ["total_spent"] = c.TotalSpent,
            ["frequency"] = c.PurchaseFrequency,
            ["recency"] = c.DaysSinceLastPurchase
        }).ToList();
        
        var (segments, characteristics) = segmentation.SegmentCustomers(pythonData);
        
        return new CustomerSegmentationResult
        {
            CustomerSegments = segments,
            SegmentCharacteristics = characteristics
        };
    }
    
    public MarketingStrategy GetMarketingStrategy(int segmentId)
    {
        var segmentation = _python.CustomerSegmentation();
        var strategy = segmentation.RecommendMarketingStrategy(segmentId);
        
        return new MarketingStrategy
        {
            Name = strategy["name"],
            Strategy = strategy["strategy"],
            Message = strategy["message"]
        };
    }
}
```

### Financial Risk Analysis

**Scenario**: A fintech company needs to assess loan default risk using machine learning.

```python
# risk_analysis.py
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import StandardScaler
import pickle
import os

def train_risk_model(training_data: list[dict[str, float]], labels: list[int]) -> str:
    """
    Train a risk assessment model and save it to disk.
    
    Returns: Model file path
    """
    # Prepare features
    features = []
    for record in training_data:
        features.append([
            record['income'],
            record['debt_ratio'],
            record['credit_score'],
            record['employment_years'],
            record['loan_amount']
        ])
    
    X = np.array(features)
    y = np.array(labels)
    
    # Scale features
    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)
    
    # Train model
    model = RandomForestClassifier(n_estimators=100, random_state=42)
    model.fit(X_scaled, y)
    
    # Save model and scaler
    model_path = "risk_model.pkl"
    scaler_path = "risk_scaler.pkl"
    
    with open(model_path, 'wb') as f:
        pickle.dump(model, f)
    
    with open(scaler_path, 'wb') as f:
        pickle.dump(scaler, f)
    
    return model_path

def assess_risk(applicant_data: dict[str, float]) -> tuple[float, str]:
    """
    Assess risk for a loan applicant.
    
    Returns: (risk_probability, risk_category)
    """
    # Load model and scaler
    with open("risk_model.pkl", 'rb') as f:
        model = pickle.load(f)
    
    with open("risk_scaler.pkl", 'rb') as f:
        scaler = pickle.load(f)
    
    # Prepare features
    features = np.array([[
        applicant_data['income'],
        applicant_data['debt_ratio'],
        applicant_data['credit_score'],
        applicant_data['employment_years'],
        applicant_data['loan_amount']
    ]])
    
    # Scale and predict
    features_scaled = scaler.transform(features)
    risk_prob = float(model.predict_proba(features_scaled)[0][1])
    
    # Categorize risk
    if risk_prob < 0.3:
        category = "Low Risk"
    elif risk_prob < 0.7:
        category = "Medium Risk"
    else:
        category = "High Risk"
    
    return risk_prob, category
```

## Text Processing and NLP

### Content Moderation System

**Scenario**: A social media platform needs automated content moderation.

```python
# content_moderation.py
import re
from typing import List, Dict, Tuple
from textblob import TextBlob
import nltk
from nltk.corpus import stopwords

# Download required NLTK data (do this once)
try:
    nltk.data.find('corpora/stopwords')
except LookupError:
    nltk.download('stopwords')

def analyze_content(text: str) -> dict[str, any]:
    """
    Analyze content for moderation purposes.
    
    Returns analysis including sentiment, toxicity indicators, etc.
    """
    # Basic sentiment analysis
    blob = TextBlob(text)
    sentiment_score = blob.sentiment.polarity
    
    # Toxicity indicators (simplified)
    toxic_patterns = [
        r'\b(hate|kill|die|stupid|idiot)\b',
        r'[A-Z]{3,}',  # Excessive caps
        r'(.)\1{3,}',  # Repeated characters
    ]
    
    toxicity_score = 0
    flagged_patterns = []
    
    for pattern in toxic_patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        if matches:
            toxicity_score += len(matches)
            flagged_patterns.append(pattern)
    
    # Content classification
    word_count = len(text.split())
    contains_urls = bool(re.search(r'http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+', text))
    
    # Determine action
    if toxicity_score > 3 or sentiment_score < -0.5:
        action = "block"
    elif toxicity_score > 1 or sentiment_score < -0.2:
        action = "review"
    else:
        action = "approve"
    
    return {
        "sentiment_score": sentiment_score,
        "toxicity_score": toxicity_score,
        "flagged_patterns": flagged_patterns,
        "word_count": word_count,
        "contains_urls": contains_urls,
        "recommended_action": action,
        "confidence": min(1.0, abs(sentiment_score) + (toxicity_score * 0.1))
    }

def extract_keywords(text: str, max_keywords: int = 10) -> list[str]:
    """Extract keywords from text."""
    blob = TextBlob(text)
    words = blob.words.lower()
    
    # Remove stopwords
    stop_words = set(stopwords.words('english'))
    keywords = [word for word in words if word not in stop_words and len(word) > 3]
    
    # Count frequency
    word_freq = {}
    for word in keywords:
        word_freq[word] = word_freq.get(word, 0) + 1
    
    # Sort by frequency
    sorted_keywords = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)
    
    return [word for word, freq in sorted_keywords[:max_keywords]]
```

```csharp
// C# Usage in a web API
[ApiController]
[Route("api/[controller]")]
public class ContentModerationController : ControllerBase
{
    private readonly IPythonEnvironment _python;
    
    public ContentModerationController(IPythonEnvironment python)
    {
        _python = python;
    }
    
    [HttpPost("analyze")]
    public ActionResult<ContentAnalysisResult> AnalyzeContent([FromBody] ContentSubmission submission)
    {
        var moderation = _python.ContentModeration();
        
        var analysis = moderation.AnalyzeContent(submission.Text);
        var keywords = moderation.ExtractKeywords(submission.Text);
        
        var result = new ContentAnalysisResult
        {
            SentimentScore = (double)analysis["sentiment_score"],
            ToxicityScore = (long)analysis["toxicity_score"],
            RecommendedAction = (string)analysis["recommended_action"],
            Confidence = (double)analysis["confidence"],
            Keywords = keywords,
            ContainsUrls = (bool)analysis["contains_urls"]
        };
        
        return Ok(result);
    }
}
```

## Image Processing and Computer Vision

### Image Quality Assessment

**Scenario**: An e-commerce platform needs to automatically assess product image quality.

```python
# image_quality.py
import cv2
import numpy as np
from typing import Tuple, Dict
import base64

def analyze_image_quality(image_base64: str) -> dict[str, float]:
    """
    Analyze image quality metrics.
    
    Args:
        image_base64: Base64 encoded image data
    
    Returns:
        Dictionary with quality metrics
    """
    # Decode base64 image
    image_data = base64.b64decode(image_base64)
    nparr = np.frombuffer(image_data, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    
    if image is None:
        return {"error": 1.0}
    
    # Convert to grayscale for some analyses
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    
    # Sharpness (Laplacian variance)
    laplacian = cv2.Laplacian(gray, cv2.CV_64F)
    sharpness = laplacian.var()
    
    # Brightness
    brightness = np.mean(gray)
    
    # Contrast (standard deviation)
    contrast = np.std(gray)
    
    # Resolution
    height, width = gray.shape
    resolution_score = min(1.0, (height * width) / (1920 * 1080))  # Normalized to 1080p
    
    # Color distribution (for color images)
    color_channels = cv2.split(image)
    color_variance = np.mean([np.var(channel) for channel in color_channels])
    
    # Overall quality score (0-100)
    quality_score = min(100, (
        (min(sharpness / 100, 1.0) * 25) +
        (min(contrast / 50, 1.0) * 25) +
        (resolution_score * 25) +
        (min(color_variance / 1000, 1.0) * 25)
    ))
    
    return {
        "sharpness": float(sharpness),
        "brightness": float(brightness),
        "contrast": float(contrast),
        "resolution_score": float(resolution_score),
        "color_variance": float(color_variance),
        "quality_score": float(quality_score),
        "width": float(width),
        "height": float(height)
    }

def detect_objects(image_base64: str) -> list[dict[str, any]]:
    """
    Detect objects in image using basic OpenCV methods.
    """
    # Decode image
    image_data = base64.b64decode(image_base64)
    nparr = np.frombuffer(image_data, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    
    if image is None:
        return []
    
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    
    # Simple edge detection
    edges = cv2.Canny(gray, 50, 150)
    
    # Find contours
    contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    
    objects = []
    for i, contour in enumerate(contours):
        area = cv2.contourArea(contour)
        if area > 1000:  # Filter small objects
            x, y, w, h = cv2.boundingRect(contour)
            objects.append({
                "id": i,
                "x": int(x),
                "y": int(y),
                "width": int(w),
                "height": int(h),
                "area": float(area)
            })
    
    return objects
```

## IoT and Sensor Data Processing

### Smart Home Data Analysis

**Scenario**: A smart home system needs to analyze sensor data for automation and alerts.

```python
# sensor_analysis.py
import json
from datetime import datetime, timedelta
from typing import List, Dict, Optional
import numpy as np

def analyze_temperature_patterns(sensor_data: list[dict[str, any]]) -> dict[str, any]:
    """
    Analyze temperature sensor data for patterns and anomalies.
    
    Args:
        sensor_data: List of sensor readings with timestamp, room, temperature
    """
    # Group by room
    room_data = {}
    for reading in sensor_data:
        room = reading['room']
        if room not in room_data:
            room_data[room] = []
        room_data[room].append({
            'timestamp': reading['timestamp'],
            'temperature': reading['temperature']
        })
    
    analysis = {}
    
    for room, readings in room_data.items():
        temperatures = [r['temperature'] for r in readings]
        
        if not temperatures:
            continue
            
        avg_temp = np.mean(temperatures)
        std_temp = np.std(temperatures)
        min_temp = np.min(temperatures)
        max_temp = np.max(temperatures)
        
        # Detect anomalies (values outside 2 standard deviations)
        anomalies = []
        for reading in readings:
            temp = reading['temperature']
            if abs(temp - avg_temp) > 2 * std_temp:
                anomalies.append({
                    'timestamp': reading['timestamp'],
                    'temperature': temp,
                    'deviation': abs(temp - avg_temp)
                })
        
        # Trend analysis (simplified)
        if len(temperatures) > 1:
            trend = "increasing" if temperatures[-1] > temperatures[0] else "decreasing"
        else:
            trend = "stable"
        
        analysis[room] = {
            'average_temperature': float(avg_temp),
            'temperature_range': float(max_temp - min_temp),
            'stability': float(std_temp),
            'anomalies': anomalies,
            'trend': trend,
            'comfort_score': float(max(0, min(100, 100 - abs(avg_temp - 22) * 5)))  # Optimal at 22°C
        }
    
    return analysis

def generate_automation_rules(analysis: dict[str, any]) -> list[dict[str, str]]:
    """Generate automation rules based on sensor analysis."""
    rules = []
    
    for room, data in analysis.items():
        avg_temp = data['average_temperature']
        comfort_score = data['comfort_score']
        
        if avg_temp > 25:
            rules.append({
                'room': room,
                'action': 'turn_on_ac',
                'reason': f'Temperature too high ({avg_temp:.1f}°C)',
                'priority': 'high'
            })
        elif avg_temp < 18:
            rules.append({
                'room': room,
                'action': 'turn_on_heating',
                'reason': f'Temperature too low ({avg_temp:.1f}°C)',
                'priority': 'high'
            })
        
        if comfort_score < 50:
            rules.append({
                'room': room,
                'action': 'adjust_climate',
                'reason': f'Low comfort score ({comfort_score:.0f})',
                'priority': 'medium'
            })
        
        if len(data['anomalies']) > 0:
            rules.append({
                'room': room,
                'action': 'check_sensor',
                'reason': f'Temperature anomalies detected ({len(data["anomalies"])} readings)',
                'priority': 'low'
            })
    
    return rules
```

```csharp
// C# Usage in an IoT application
public class SmartHomeService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<SmartHomeService> _logger;
    
    public SmartHomeService(IPythonEnvironment python, ILogger<SmartHomeService> logger)
    {
        _python = python;
        _logger = logger;
    }
    
    public async Task<SmartHomeAnalysis> AnalyzeSensorDataAsync(List<SensorReading> readings)
    {
        var analyzer = _python.SensorAnalysis();
        
        // Convert to Python format
        var pythonData = readings.Select(r => new Dictionary<string, object>
        {
            ["timestamp"] = r.Timestamp.ToString("O"),
            ["room"] = r.Room,
            ["temperature"] = r.Temperature
        }).ToList();
        
        var analysis = analyzer.AnalyzeTemperaturePatterns(pythonData);
        var rules = analyzer.GenerateAutomationRules(analysis);
        
        // Log important findings
        foreach (var rule in rules.Where(r => r["priority"].ToString() == "high"))
        {
            _logger.LogWarning("High priority automation rule: {Action} in {Room} - {Reason}", 
                rule["action"], rule["room"], rule["reason"]);
        }
        
        return new SmartHomeAnalysis
        {
            RoomAnalysis = analysis,
            AutomationRules = rules
        };
    }
}
```

## Next Steps

- [Explore sample projects](sample-projects.md)
- [Learn best practices](best-practices.md)
- [Check out advanced patterns](../advanced/advanced-usage.md)
