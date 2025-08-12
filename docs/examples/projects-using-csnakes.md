# Projects using CSnakes

CSnakes is being used in various projects to integrate Python code into C# applications. Here are some notable examples:

## TransformersSharp

**Project:** [TransformersSharp](https://tonybaloney.github.io/TransformersSharp/)  
**Repository:** [GitHub - TransformersSharp](https://github.com/tonybaloney/TransformersSharp)  
**Author:** Tony Baloney

TransformersSharp is a C# wrapper for Hugging Face Transformers that demonstrates the power of CSnakes for AI/ML integration. This project provides a .NET-friendly interface to the popular Python transformers library, enabling C# developers to leverage state-of-the-art machine learning models without leaving the .NET ecosystem.

### Key Features

- **Tokenizer API** based on Hugging Face's PreTrainedTokenizerBase
- **Pipeline Factory** for creating various ML pipelines
- **Text Generation Pipeline** for language model text generation
- **Text Classification Pipeline** for sentiment analysis and classification tasks
- **Image Classification Pipeline** for computer vision tasks
- **Object Detection Pipeline** for detecting objects in images
- **Text to Audio Pipeline** for speech synthesis
- **Automatic Speech Recognition** for transcribing audio to text
- **Sentence Transformers** for generating embeddings

### Why CSnakes?

TransformersSharp showcases several key advantages of CSnakes:

1. **Automatic Dependency Management**: The project automatically fetches Python, PyTorch, and Hugging Face Transformers - no manual Python installation required
2. **Type Safety**: Strong typing between C# and Python code with automatic type conversion
3. **Performance**: Direct integration without the overhead of separate processes or REST APIs
4. **Ease of Use**: Simple C# API that abstracts away Python complexity

### Example Usage

```csharp
using TransformersSharp;

// Text classification
var classifier = new TextClassificationPipeline("cardiffnlp/twitter-roberta-base-sentiment-latest");
var result = classifier.Predict("I love using CSnakes with TransformersSharp!");

// Text generation
var generator = new TextGenerationPipeline("gpt2");
var generated = generator.Generate("The future of AI is", maxLength: 50);

// Image classification
var imageClassifier = new ImageClassificationPipeline("google/vit-base-patch16-224");
var prediction = imageClassifier.Predict("path/to/image.jpg");
```

### Impact

TransformersSharp demonstrates how CSnakes enables:

- **Enterprise AI Integration**: Bringing cutting-edge AI models into .NET enterprise applications
- **Rapid Prototyping**: Quick experimentation with ML models in familiar C# environment
- **Production Deployment**: Reliable deployment of Python-based AI models in .NET infrastructure
- **Developer Productivity**: Eliminating the need for separate Python environments and API bridges

This project serves as an excellent reference for developers looking to integrate complex Python AI/ML libraries into their .NET applications using CSnakes.

---

Got a project using CSnakes you want featured here? Submit an issue on the [CSnakes GitHub repository](https://github.com/tonybaloney/CSnakes)!