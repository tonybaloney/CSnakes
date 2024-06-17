from mistral_inference.model import Transformer
from mistral_inference.generate import generate

from mistral_common.tokens.tokenizers.mistral import MistralTokenizer
from mistral_common.protocol.instruct.messages import UserMessage
from mistral_common.protocol.instruct.request import ChatCompletionRequest


def invoke_mistral_inference(messages: list[str], lang: str = "en-US", temperature=0.0) -> str:
	tokenizer = MistralTokenizer.from_file(f"{mistral_models_path}/tokenizer.model.v3")
	model = Transformer.from_folder(mistral_models_path)

	completion_request = ChatCompletionRequest(messages=[UserMessage(content=message) for message in messages])

	tokens = tokenizer.encode_chat_completion(completion_request).tokens

	out_tokens, _ = generate([tokens], model, max_tokens=64, temperature=temperature, eos_id=tokenizer.instruct_tokenizer.tokenizer.eos_id)
	return tokenizer.instruct_tokenizer.tokenizer.decode(out_tokens[0])
