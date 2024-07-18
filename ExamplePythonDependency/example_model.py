import re
import torch
from transformers import pipeline


def invoke_model(input_string: str) -> str:
    pipe = pipeline("text-generation", model="AI-MO/NuminaMath-7B-TIR", torch_dtype=torch.bfloat16, device_map="auto")

    messages = [
        {"role": "user", "content": input_string},
    ]
    prompt = pipe.tokenizer.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)

    gen_config = {
        "max_new_tokens": 1024,
        "do_sample": False,
        "stop_strings": ["```output"], # Generate until Python code block is complete
        "tokenizer": pipe.tokenizer,
    }

    outputs = pipe(prompt, **gen_config)
    text = outputs[0]["generated_text"]
    return text