class EngineSoundGenerator extends AudioWorkletProcessor {
	constructor(opts) {
		super();

		this.sampleRate = sampleRate;

		this.rpm = 3000;
		this.throttle = 1;

		this.freqScale = opts.processorOptions.freqScale;
		this.baseDuty = opts.processorOptions.baseDuty;
		this.throttleDuty = opts.processorOptions.throttleDuty;
		this.waves = opts.processorOptions.waves;
		this.phase = new Array(this.waves.length).fill(0);

		this.port.onmessage = (e) => {
			if (e.data.rpm != undefined) {
				this.rpm = e.data.rpm;
			}
			if (e.data.throttle != undefined) {
				this.throttle = e.data.throttle;
			}
		}
	}

	sample() {
		let baseFreq = this.rpm * this.freqScale;
		let sub = 0;
		let totalVolume = 0;
		for (let j = 0; j < this.waves.length; j++) {
			let wave = this.waves[j];
			this.phase[j] += baseFreq * wave.overtone / this.sampleRate; 
			if (this.phase[j] >= 100.0) this.phase[j] -= 100.0;
			if (!this.ff) {
				console.log(this);
				this.ff = true;
			}
			totalVolume += wave.volume;
			sub += wave.volume * Math.pow(
				Math.sin(this.phase[j] + wave.offset),
				Math.floor(
					wave.baseDuty + this.baseDuty * wave.dutyScale +
					(this.throttle > 0.1 ? 1 : 0) * this.throttleDuty * wave.throttleDutyScale
				)
			);
		}

		return sub / totalVolume;
	}

	process(inputs, outputs, parameters) {
		const output = outputs[0];

		for (let i = 0; i < output[0].length; i++) {
			const v = this.sample();
			for (let j = 0; j < output.length; j++) output[j][i] = v;
		}

		return true;
	}
}

registerProcessor("EngineSoundGenerator", EngineSoundGenerator);

