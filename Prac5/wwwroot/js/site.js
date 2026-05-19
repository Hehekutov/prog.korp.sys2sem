document.querySelectorAll("[data-production-form]").forEach((form) => {
    const productSelect = form.querySelector("[data-product-select]");
    const quantityInput = form.querySelector("[data-quantity-input]");
    const lineSelect = form.querySelector("[data-line-select]");
    const output = form.querySelector("[data-estimate-output]");

    const updateEstimate = () => {
        const productOption = productSelect?.selectedOptions[0];
        const lineOption = lineSelect?.selectedOptions[0];
        const timePerUnit = Number(productOption?.dataset.time ?? 0);
        const quantity = Number(quantityInput?.value ?? 0);
        const efficiency = Math.max(Number(lineOption?.dataset.efficiency ?? 1), 0.5);
        const minutes = Math.ceil((quantity * timePerUnit) / efficiency);

        if (!output) {
            return;
        }

        if (!Number.isFinite(minutes) || minutes <= 0) {
            output.textContent = "Расчет: 0 мин";
            return;
        }

        const end = new Date(Date.now() + minutes * 60000);
        output.textContent = `Расчет: ${minutes} мин, до ${end.toLocaleString("ru-RU", {
            day: "2-digit",
            month: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        })}`;
    };

    productSelect?.addEventListener("change", updateEstimate);
    quantityInput?.addEventListener("input", updateEstimate);
    lineSelect?.addEventListener("change", updateEstimate);
    updateEstimate();
});
